
r'''
See https://github.com/jentic/arazzo-engine/blob/main/runner/README.md

$env:BANK_APIKEY_HEADER = "Lifetime Subscription"
python -m pip install arazzo-runner
python .\.arazzo\run.py .\.arazzo\v1_arazzo.yaml --use-zscaler-cert
'''

import argparse
from dataclasses import asdict
import json
from pathlib import Path
import ssl

import requests
from arazzo_runner import ArazzoRunner
import urllib3.util.ssl_


DEFAULT_ARAZZO_PATH = Path(__file__).with_name("v1_arazzo.yaml")
CA_CERT_PATH = Path(__file__).resolve().parents[1] / ".certs" / "ZscalerRootCA.crt"


def create_session(use_zscaler_cert):
	session = requests.Session()
	if use_zscaler_cert:
		ssl.VERIFY_X509_STRICT = 0
		urllib3.util.ssl_.VERIFY_X509_STRICT = 0
		session.verify = str(CA_CERT_PATH)
	return session


def parse_args():
	parser = argparse.ArgumentParser()
	parser.add_argument("arazzo_path", nargs="?", type=Path, default=DEFAULT_ARAZZO_PATH)
	parser.add_argument("--use-zscaler-cert", action="store_true")
	return parser.parse_args()


def main():
	args = parse_args()
	runner = ArazzoRunner.from_arazzo_path(
		str(args.arazzo_path),
		http_client=create_session(args.use_zscaler_cert),
	)
	result = runner.execute_workflow("getAndUpdateBank", {"page": 1, "pageSize": 21})
	print(json.dumps(asdict(result), indent=2, default=str))


if __name__ == "__main__":
	main()