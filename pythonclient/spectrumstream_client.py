from signalrcore.hub_connection_builder import HubConnectionBuilder
import base64
import threading
import struct

current_scan = {
    "scan_id": None,
    "count": 0,
    "total_chunks": 0,
    "chunk_size": 0,
    "total_bytes": 0,
    "chunks": {}
}

scan_done = threading.Event()

def on_scan_started(args):
    msg = args[0]
    current_scan["scan_id"] = msg["scanId"]
    current_scan["count"] = msg["count"]
    current_scan["total_chunks"] = msg["totalChunks"]
    current_scan["total_bytes"] = msg["totalBytes"]
    current_scan["chunk_size"] = msg["chunkSize"]
    current_scan["chunks"] = {}
    
    print(f"Scan started: {current_scan['scan_id']}, chunks={current_scan['total_chunks']}")

def on_scan_chunk(args):
    msg = args[0]
    chunk_index = msg["chunkIndex"]
    data_field = msg["data"]
    chunk_length = msg["chunkLength"]

    if isinstance(data_field, str):
        chunk_bytes = base64.b64decode(data_field)
    elif isinstance(data_field, list):
        chunk_bytes = bytes(data_field)
    else:
        chunk_bytes = bytes(data_field)

    current_scan["chunks"][chunk_index] = chunk_bytes
    print(f"Chunk {chunk_index + 1}/{msg['totalChunks']} received")

def on_scan_completed(args):
    msg = args[0]
    print(f"Scan completed: {msg['scanId']}")

    total_chunks = current_scan["total_chunks"]
    payload = b"".join(current_scan["chunks"][i] for i in range(total_chunks))

    count = current_scan["count"]

    expected_bytes = count * 24
    if len(payload) != expected_bytes:
        raise RuntimeError(f"Unexpected payload size. got={len(payload)}, expected={expected_bytes}")

    all_values = struct.unpack("<ddd" * count, payload)

    print(f"Reconstructed values: {len(all_values)}")
    scan_done.set()

hub = (
    HubConnectionBuilder()
    .with_url("http://localhost:5050/scanHub")
    .build()
)

hub.on("ScanStarted", on_scan_started)
hub.on("ScanChunk", on_scan_chunk)
hub.on("ScanCompleted", on_scan_completed)

hub.start()

input("Press Enter to start scan...")
hub.send("StartScan", [])

scan_done.wait()
hub.stop()