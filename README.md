# Steam-Inventory-Appraiser

## Instructions
    1. Copy .env.example as .env
    2. Change values in .env
    3. ```bash docker-compose up --build```

## Ports
API available at localhost:5001
PostgreSQL available at localhost:5433
Frontend available at localhost:3000

## To-do
    - [ ] Get data from steam and csfloat 
    - [ ] Make endpoint with required data only in backend (C#) 
    - [ ] Database API in backend (C#) 
    - [ ] XML and JSON schema
    - [ ] Frontend:
        - [ ] Get Steam Profile ID or extract Profile ID from steamcommunity.com link from user
        - [ ] Save user profile and inventory to database
        - [ ] Add "Update data" button
        - [ ] Display items from inventory worth at least 0.01$
        - [ ] Make frontend visually appealing
        - [ ] Export data from db as .json or .xml
        - [ ] Import data to db as .json or .xml
