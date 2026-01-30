# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build arguments for environment variables (passed at build time)
# These match the variable names used in the frontend code
ARG VITE_API_URL=http://localhost:5050
ARG VITE_VIDEO_CATALOGUE_API=http://localhost:5002
ARG VITE_CHAT_SERVICE_URL=http://localhost:5004

# Set environment variables for build
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_VIDEO_CATALOGUE_API=$VITE_VIDEO_CATALOGUE_API
ENV VITE_CHAT_SERVICE_URL=$VITE_CHAT_SERVICE_URL

# Build the application
RUN npm run build

# Production stage - serve with nginx
FROM nginx:alpine AS production

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built assets from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
