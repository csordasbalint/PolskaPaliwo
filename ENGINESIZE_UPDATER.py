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

collection.update_many({"EngineSize": ""}, {"$set": {"EngineSize": "E"}})

client.close()

print("script ended")

end_time = time_measure.time()
run_time = end_time - start_time
print(f"Run time: {run_time:.2f} seconds")

print("==================================================================================")