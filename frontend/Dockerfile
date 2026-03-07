# Build stage
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci --silent
COPY . .
# Build production bundle
RUN npm run build -- --configuration production --output-path=dist

# Runtime stage
FROM nginx:alpine
# Clean default files and copy built files into nginx html root
RUN rm -rf /usr/share/nginx/html/*
COPY --from=build /app/dist/. /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
