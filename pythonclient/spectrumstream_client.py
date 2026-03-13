import base64
import threading
import queue
import time
from dataclasses import dataclass, field
from typing import Optional

import numpy as np
from signalrcore.hub_connection_builder import HubConnectionBuilder
from datetime import datetime, timezone

# =========================
# Configuration
# =========================

SERVER_URL = "http://localhost:5050/scanHub"


# =========================
# Models
# =========================

@dataclass
class ActiveScan:
    scan_id: str
    count: int
    total_bytes: int
    total_chunks: int
    chunk_size: int
    buffer: bytearray
    view: memoryview
    received_chunks: int = 0
    received_bytes: int = 0
    chunk_indexes: set = field(default_factory=set)


# =========================
# Client
# =========================

class AcquisitionClient:
    def __init__(self, server_url: str) -> None:
        self._server_url = server_url
        self._hub = (
            HubConnectionBuilder()
            .with_url(server_url)
            .build()
        )

        self._lock = threading.Lock()
        self._running = False
        self._active_scans: dict[str, ActiveScan] = {}
        self._completed_scan_queue: queue.Queue[ActiveScan] = queue.Queue()
        self._processor_stop = threading.Event()
        self._processor_thread = threading.Thread(
            target=self._process_completed_scans_loop,
            name="scan-processor",
            daemon=True
        )

        self._register_handlers()

    def _register_handlers(self) -> None:
        self._hub.on("scanStarted", self._on_scan_started)
        self._hub.on("scanChunk", self._on_scan_chunk)
        self._hub.on("scanCompleted", self._on_scan_completed)
        self._hub.on("scanFailed", self._on_scan_failed)

    def connect(self) -> None:
        self._hub.start()
        self._processor_thread.start()
        print(f"Connected to {self._server_url}")

    def disconnect(self) -> None:
        self._processor_stop.set()
        self._completed_scan_queue.put(None)  # sentinel
        self._processor_thread.join(timeout=2.0)
        self._hub.stop()
        print("Disconnected")

    def start_acquisition(self, ms_settings_file: str) -> None:
        payload = {
            "MSSettingsFile": ms_settings_file
        }

        self._hub.send("startAcquisition", [payload])

        with self._lock:
            self._running = True

        print(f"startAcquisition sent. MSSettingsFile = {ms_settings_file}")

    def stop_acquisition(self) -> None:
        self._hub.send("stopAcquisition", [])

        with self._lock:
            self._running = False

        print("stopAcquisition sent.")

    # =========================
    # SignalR handlers
    # =========================

    def _on_scan_started(self, args) -> None:
        msg = args[0]

        scan_id = msg["scanId"]
        count = int(msg["count"])
        total_bytes = int(msg["totalBytes"])
        total_chunks = int(msg["totalChunks"])
        chunk_size = int(msg["chunkSize"])


        buffer = bytearray(total_bytes)
        scan = ActiveScan(
            scan_id=scan_id,
            count=count,
            total_bytes=total_bytes,
            total_chunks=total_chunks,
            chunk_size=chunk_size,
            buffer=buffer,
            view=memoryview(buffer)
        )

        with self._lock:
            self._active_scans[scan_id] = scan

        print(
            f"[ScanStarted] scanId={scan_id}, "
            f"Count={count}, chunkSize={chunk_size}, "
            f"totalBytes={total_bytes}, totalChunks={total_chunks}"
        )

    def _on_scan_chunk(self, args) -> None:
        msg = args[0]

        scan_id = msg["scanId"]
        offset = int(msg["offset"])
        count = int(msg["count"])
        chunkIndex = int(msg["chunkIndex"])

        with self._lock:
            scan = self._active_scans.get(scan_id)

        if scan is None:
            print(f"[ScanChunk] Unknown scanId={scan_id}")
            return

        if chunkIndex not in scan.chunk_indexes:
            scan.chunk_indexes.add(chunkIndex)
        else:
            print(f"[ScanChunk] Duplicate chunkIndex={chunkIndex} for scanId={scan_id}")
            return
        
        data_field = msg["data"]

        # Fast path depends on how the Python SignalR client materializes binary payloads.
        if isinstance(data_field, str):
            # base64 text
            chunk_bytes = base64.b64decode(data_field)
            scan.view[offset:offset + count] = chunk_bytes[:count]
        else:
            # usually list[int] or bytes-like
            chunk_bytes = bytes(data_field)
            scan.view[offset:offset + count] = chunk_bytes[:count]

        scan.received_chunks = scan.received_chunks + 1
        scan.received_bytes = scan.received_bytes + count

    def _on_scan_completed(self, args) -> None:
        msg = args[0]
        scan_id = msg["scanId"]
        time_stamp = msg.get("timeStamp")

        with self._lock:
            scan = self._active_scans.pop(scan_id, None)

        if scan is None:
            print(f"[ScanCompleted] Unknown scanId={scan_id}")
            return

        if scan.received_bytes != scan.total_bytes:
            print(
                f"[ScanCompleted] WARNING: scanId={scan_id}, "
                f"receivedBytes={scan.received_bytes}, expectedBytes={scan.total_bytes}"
            )

        self._completed_scan_queue.put(scan)
        
        now = datetime.now(timezone.utc)
        
        formatted = now.strftime("%M:%S.") + f"{now.microsecond // 1000:03d}"
        
        print(f"[ScanCompleted] scanId={scan_id}, time_stamp={time_stamp} now={formatted}")

    def _on_scan_failed(self, args) -> None:
        msg = args[0]
        scan_id = msg.get("scanId")
        error = msg.get("error")

        with self._lock:
            if scan_id in self._active_scans:
                scan = self._active_scans.pop(scan_id)
                scan.view.release()

        print(f"[ScanFailed] scanId={scan_id}, error={error}")

    # =========================
    # Processing
    # =========================

    def _process_completed_scans_loop(self) -> None:
        while not self._processor_stop.is_set():
            item = self._completed_scan_queue.get()
            if item is None:
                break

            try:
                self._process_completed_scan(item)
            except Exception as ex:
                print(f"[Processor] Error while processing scan {item.scan_id}: {ex}")
            finally:
                item.view.release()

    def _process_completed_scan(self, scan: ActiveScan) -> None:
        # Zero-copy NumPy view over the received bytes.
        values = np.frombuffer(scan.buffer, dtype="<f8")

        values = values.reshape(-1, 2)

        expected_count = scan.count
        if values.shape[0] != expected_count:
            raise RuntimeError(
                f"Unexpected double count for scan {scan.scan_id}. "
                f"got={values.size}, expected={expected_count}"
            )

        mass, intensity = values[0]
    
        print(mass)
        print(intensity)
        
        print(f"[Processor] Processed scan {scan.scan_id}: mass={mass}, intensity={intensity}"  )
        # Put the processing block here


# =========================
# Main
# =========================

def main() -> None:
    ms_settings_file = input("MSSettingsFile: ").strip()
    if not ms_settings_file:
        print("MSSettingsFile is required.")
        return

    client = AcquisitionClient(SERVER_URL)

    try:
        client.connect()
        client.start_acquisition(ms_settings_file)

        print("Acquisition is running.")
        print("Press ENTER to stop acquisition.")
        input()

        client.stop_acquisition()

        # Give the server a short grace period to deliver any final scans.
        time.sleep(1.0)

    finally:
        client.disconnect()


if __name__ == "__main__":
    main()