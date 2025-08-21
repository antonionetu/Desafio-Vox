FROM node:20 AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

FROM node:20
WORKDIR /app

COPY --from=build /app ./
EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host"]
