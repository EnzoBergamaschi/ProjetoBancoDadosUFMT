FROM debian:bookworm-slim
WORKDIR /app

# Install dependencies for .NET self-contained
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libicu-dev \
    libssl-dev \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy locally published self-contained app
COPY publish/ .

# Ensure executable permissions
RUN chmod +x ./SimpleCrud.Api

EXPOSE 8080
ENTRYPOINT ["./SimpleCrud.Api"]
