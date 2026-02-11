FROM node:20-alpine
WORKDIR /app

COPY package*.json ./
RUN npm install

# We'll mount the rest of the source as a volume in docker-compose
EXPOSE 5173
ENTRYPOINT ["npm", "run", "dev", "--", "--host"]
