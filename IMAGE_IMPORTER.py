import time as time_measure
start_time = time_measure.time()

print("==================================================================================")
print("script started")

import os
from pymongo import MongoClient
from bson import Binary

# Connect to MongoDB
client = MongoClient('mongodb://localhost:27017/')
db = client['PolskaPaliwoDatabase']
collection = db['CarAdvertisements']

# Path to the folder containing images
image_folder_path = r'C:\Users\Csordi\Desktop\Testing Folder\Images'

# Get a list of all image files in the folder
image_files = [f for f in os.listdir(image_folder_path) if os.path.isfile(os.path.join(image_folder_path, f))]
image_files_iter = iter(image_files)

try:
    # Iterate through all documents in the collection and update each with the next available image
    for document in collection.find():
        try:
            # Get the next image file from the iterator
            image_filename = next(image_files_iter)
            image_path = os.path.join(image_folder_path, image_filename)

            # Read image file as binary data
            with open(image_path, 'rb') as image_file:
                image_binary = Binary(image_file.read())

            # Set the content type to PNG
            content_type = 'image/png'

            # Update the current document with image data and content type
            update = {'$set': {'Data': image_binary, 'ContentType': content_type}}
            collection.update_one({'_id': document['_id']}, update)
            print(f"Updated document with image file: {image_filename}")

        except StopIteration:
            print("No more image files available to associate with documents.")
            break

    print('Image data update process completed.')

except Exception as e:
    print(f"An error occurred: {e}")

print("script ended")

end_time = time_measure.time()
run_time = end_time - start_time
print(f"Run time: {run_time:.2f} seconds")

print("==================================================================================")
