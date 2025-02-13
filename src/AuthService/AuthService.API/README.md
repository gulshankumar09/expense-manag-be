## Running the Project with Docker

### Requirements
- Docker and Docker Compose installed on your system.
- .NET SDK version 9.0 (as specified in the Dockerfile).

### Build and Run Instructions
1. Clone the repository and navigate to the project root directory.
2. Build and start the services using Docker Compose:
   ```bash
   docker-compose up --build
   ```

### Configuration
- The application runs on port `5000`. Ensure this port is available on your host machine.
- The `authservice` service is part of the `app-network` Docker network (bridge driver).

### Ports
- `authservice`: Exposes port `5000` (mapped to `5000` on the host).

### Notes
- The application runs as a non-root user (`appuser`) for enhanced security.
- No additional environment variables are required for this setup based on the provided files.