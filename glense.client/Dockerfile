# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Set build-time environment variable.
# The browser (not the container) makes API calls, so this must be an address the
# browser can reach. In the K8s local-dev setup, the gateway is port-forwarded to
# localhost:5050 — that's what the built JS bundle will call.
ENV VITE_API_URL=http://localhost:5050

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the app
RUN npm run build

# Production stage
FROM nginx:alpine

# Copy built app from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx config if needed (optional)
# COPY nginx.conf /etc/nginx/nginx.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]