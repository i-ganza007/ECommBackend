var builder = DistributedApplication.CreateBuilder(args);

// Local SMTP catcher: every email the consumer sends lands in its web UI instead of a real inbox.
var mailpit = builder.AddContainer("mailpit", "axllent/mailpit")
    .WithHttpEndpoint(targetPort: 8025, name: "ui")
    .WithEndpoint(targetPort: 1025, name: "smtp", scheme: "tcp");

var smtp = mailpit.GetEndpoint("smtp");

// Both projects still read SecretKeys:PrimaryConn from their own user secrets.
builder.AddProject<Projects.ECommBackend>("ecommbackend");

builder.AddProject<Projects.ConsumerProject>("emailconsumer")
    // Overrides the Smtp section so mail goes to Mailpit while running under Aspire.
    .WithEnvironment("Smtp__Host", smtp.Property(EndpointProperty.Host))
    .WithEnvironment("Smtp__Port", smtp.Property(EndpointProperty.Port))
    .WithEnvironment("Smtp__Security", "None")
    .WaitFor(mailpit);

builder.Build().Run();
