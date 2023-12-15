import time as time_measure
start_time = time_measure.time()

print("==================================================================================")
print("script started")

import os
import shutil
import pandas as pd
from bing_image_downloader import downloader

input_file_path = r"C:/Users/Csordi/Downloads/car_to_db.csv"
df = pd.read_csv(input_file_path, delimiter=';', index_col=False)

df["TO_SEARCH"] = df.apply(lambda row: f"{row.Brand} {row.Model} {row.ProductionYear} {row.Colour} colour", axis=1)
string_list = df["TO_SEARCH"].tolist()

output_file_path = r'C:\Users\Csordi\Desktop\Testing Folder\Images'

max_attempts = 15  # Maximum number of attempts to download an image

for index, item in enumerate(string_list, start=1):
    current_attempt = 0

    while current_attempt < max_attempts:
        try:
            downloader.download(item, limit=1, output_dir=output_file_path, adult_filter_off=True, force_replace=False, timeout=160, verbose=True)

            subfolder_path = os.path.join(output_file_path, item)
            files = os.listdir(subfolder_path)

            new_filename = f"{str(index).zfill(7)}_{item.replace(' ', '_').lower()}.png"
            src = os.path.join(subfolder_path, files[0])
            dst = os.path.join(output_file_path, new_filename)
            shutil.move(src, dst)

            os.rmdir(subfolder_path)
            break  # Image downloaded successfully, break out of the while loop

        except Exception as e:
            print(f"Failed to download image for '{item}'. Retrying...")
            current_attempt += 1
            if current_attempt == max_attempts:
                print(f"Skipping '{item}'. Failed to download after multiple attempts.")
                break  # Image download failed after multiple attempts, break out of the while loop

print("Script ended")

end_time = time_measure.time()
run_time = end_time - start_time
print(f"Run time: {run_time:.2f} seconds")

print("==================================================================================")
