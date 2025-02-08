var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AuthService_API>("authservice-api");

builder.Build().Run();
