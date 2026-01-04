# Steam-Inventory-Appraiser

## Instructions
1. Copy .env.example as .env
2. Change values in .env
3. ```docker-compose up --build```

## Ports
API available at localhost:5001  
Swagger at localhost:5001/swagger  
PostgreSQL available at localhost:5433  
Frontend available at localhost:3000  

## To-do
- [ ] Backend
    - [x] Get data from steam 
    - [x] Make endpoint with required data only in backend (C#) 
    - [ ] Database API in backend (C#) 
    - [ ] XML and JSON schema 
    - [x] Save user profile and inventory to database 
    - [ ] Skip items in API response worth 0.01$ or less
    - [ ] Add to /api/value/steam/profile response imageURL and rarity
- [ ] Frontend:
    - [ ] Get Steam Profile ID or extract Profile ID from steamcommunity.com link from user
    - [ ] Make a frame for an item - item image with name, value, order and border with color corresponding to item rarity  
    - [ ] Add "Update data" button
    - [ ] Make frontend visually appealing
    - [ ] Export data from db as .json or .xml
    - [ ] Import data to db as .json or .xml
