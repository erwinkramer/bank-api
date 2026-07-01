import json
import sys
from dataclasses import asdict
from pathlib import Path

import requests
import truststore
from arazzo_runner import ArazzoRunner

DEFAULT_ARAZZO_PATH = Path(__file__).with_name("v1_arazzo.yaml")

def create_http_client():
    truststore.inject_into_ssl()
    return requests.Session()

def main():
    arazzo_path = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_ARAZZO_PATH

    runner = ArazzoRunner.from_arazzo_path(
        str(arazzo_path),
        http_client=create_http_client(),
    )
    result = runner.execute_workflow("getAndUpdateBank", {"page": 1, "pageSize": 21})
    print(json.dumps(asdict(result), indent=2, default=str))

if __name__ == "__main__":
    main()