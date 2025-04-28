import datetime
import json
import pytz
import requests_cache
import requests
import sys
import os

BASE_URL = "https://pypi.org/pypi"

DEPRECATED_PACKAGES = {
    "BeautifulSoup",
    "bs4",
    "distribute",
    "django-social-auth",
    "nose",
    "pep8",
    "pycrypto",
    "pypular",
    "sklearn",
}

# Keep responses for one hour - ensure the cache directory exists or has write permissions
# For simplicity in automation, consider removing caching if issues arise.
SESSION = requests_cache.CachedSession(".temp/requests-cache", expire_after=60 * 60, backend='filesystem')


def get_json_url(package_name):
    return BASE_URL + "/" + package_name + "/json"


def annotate_wheels(packages):
    print("Getting wheel data...", file=sys.stderr)
    num_packages = len(packages)
    orange_packages = []
    for index, package in enumerate(packages):
        print(f"{index + 1}/{num_packages} {package['name']}", file=sys.stderr)
        has_abi_none_wheel = False
        has_win_arm64_wheel = False
        url = get_json_url(package["name"])

        try:
            response = SESSION.get(url)
            response.raise_for_status() # Raise HTTPError for bad responses (4xx or 5xx)
        except requests_cache.requests.exceptions.RequestException as e:
            print(f" ! Skipping {package['name']} due to request error: {e}", file=sys.stderr)
            continue
        
        try:
            data = response.json()
        except json.JSONDecodeError:
             print(f" ! Skipping {package['name']} due to invalid JSON response", file=sys.stderr)
             continue


        # Check if 'urls' key exists and is a list
        if "urls" not in data or not isinstance(data["urls"], list):
             print(f" ! Skipping {package['name']} due to missing or invalid 'urls' data", file=sys.stderr)
             continue

        for download in data["urls"]:
             # Ensure download is a dictionary and has required keys
            if not isinstance(download, dict) or "packagetype" not in download or "filename" not in download:
                continue # Skip malformed download entries

            if download["packagetype"] == "bdist_wheel":
                # Basic check for filename format before splitting
                if isinstance(download["filename"], str) and download["filename"].endswith(".whl") and '-' in download["filename"]:
                    whl_parts = download["filename"].removesuffix(".whl").split("-")
                    if len(whl_parts) >= 3: # Need at least name, version, tags
                        abi_tag = whl_parts[-2]
                        platform_tag = whl_parts[-1]

                        if abi_tag == "none":
                            has_abi_none_wheel = True

                        if "win_arm64" in platform_tag:
                            has_win_arm64_wheel = True
                    else:
                         print(f" ! Malformed wheel filename skipped for {package['name']}: {download['filename']}", file=sys.stderr)

        # If it has neither a specific win_arm64 wheel nor a generic abi_none wheel, it's "orange"
        if not has_win_arm64_wheel and not has_abi_none_wheel:
            orange_packages.append(package['name'])

    return orange_packages


def get_top_packages():
    print("Getting packages list...", file=sys.stderr)
    
    # Download the latest package list
    url = "https://hugovk.github.io/top-pypi-packages/top-pypi-packages-30-days.min.json"
    local_filename = "top-pypi-packages.json"
    print(f"Downloading latest package list from {url}...", file=sys.stderr)
    try:
        response = requests.get(url, stream=True)
        response.raise_for_status() # Raise HTTPError for bad responses (4xx or 5xx)
        with open(local_filename, 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192): 
                f.write(chunk)
        print(f"Successfully downloaded and saved to {local_filename}", file=sys.stderr)
    except requests.exceptions.RequestException as e:
        print(f"Error downloading {url}: {e}. Will try to use existing local file if available.", file=sys.stderr)
    except IOError as e:
         print(f"Error writing downloaded file to {local_filename}: {e}. Will try to use existing local file if available.", file=sys.stderr)


    # Assuming 'top-pypi-packages.json' exists in the same directory or adjust path
    try:
        with open(local_filename) as data_file:
            packages = json.load(data_file)["rows"]
    except FileNotFoundError:
        print(f"Error: {local_filename} not found and download failed.", file=sys.stderr)
        sys.exit(1)
    except json.JSONDecodeError:
        print(f"Error: Could not decode {local_filename}.", file=sys.stderr)
        sys.exit(1)


    # Rename keys
    for package in packages:
        package["downloads"] = package.pop("download_count")
        package["name"] = package.pop("project")

    return packages


def not_deprecated(package):
    return package["name"] not in DEPRECATED_PACKAGES


def remove_irrelevant_packages(packages, limit=1000): # Limit to top 200 for example
    print("Removing cruft...", file=sys.stderr)
    active_packages = list(filter(not_deprecated, packages))
    # Ensure limit doesn't exceed available packages
    actual_limit = min(limit, len(active_packages))
    return active_packages[:actual_limit]


if __name__ == "__main__":
    top_packages = get_top_packages()
    relevant_packages = remove_irrelevant_packages(top_packages)
    packages_to_test = annotate_wheels(relevant_packages)

    # Ensure log directory exists
    log_dir = "log"
    if not os.path.exists(log_dir):
        os.makedirs(log_dir)

    # Generate timestamped filename
    timestamp = datetime.datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
    output_filename = os.path.join(log_dir, f"orange_packages_list_{timestamp}.txt")

    # Write the list to the file
    try:
        with open(output_filename, 'w') as f:
            for name in packages_to_test:
                f.write(name + '\n')
        print(f"Orange package list saved to: {output_filename}", file=sys.stderr)
    except IOError as e:
        print(f"Error writing orange package list to file {output_filename}: {e}", file=sys.stderr)

    # Print only the names of the orange packages for PowerShell
    for name in packages_to_test:
        print(name)

