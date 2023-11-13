import time as time_measure
start_time = time_measure.time()

print("==================================================================================")
print("script started")

import random
from pymongo import MongoClient

client = MongoClient('mongodb://localhost:27017/')
db = client['PolskaPaliwoDatabase']
collection = db['CarAdvertisements']

#area codes
codes = [12, 13, 14, 15, 16, 17, 18, 22, 23, 24, 25,
         26, 29, 32, 33, 34, 41, 42, 43, 44, 46, 47,
         48, 52, 54, 55, 56, 58, 59, 61, 62, 63, 65,
         67, 68, 71, 74, 75, 76, 77, 81, 82, 83, 84,
         85, 86, 87, 89, 91, 94, 95]

for document in collection.find():
    code = random.choice(codes)
    first_digit = random.randint(4, 8)
    rest_of_digits = "".join(str(random.randint(0, 9)) for _ in range(6))
    new_telephone = f"+48 {first_digit}{code} {rest_of_digits}"

    #updating each document
    collection.update_one(
        {"_id": document["_id"]},
        {"$set": {"Telephone": new_telephone}}
    )

client.close()

print("script ended")

end_time = time_measure.time()
run_time = end_time - start_time
print(f"Run time: {run_time:.2f} seconds")

print("==================================================================================")
