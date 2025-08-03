# Use the official .NET SDK image for building the app (targeting .NET 6.0)
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy the project file from the subfolder (CalendarAssistant/CalendarAssistant.csproj)
COPY CalendarAssistant/CalendarAssistant.csproj ./CalendarAssistant/

# Restore dependencies
RUN dotnet restore CalendarAssistant/CalendarAssistant.csproj

# Copy the rest of the application code
COPY . ./

# Publish the application to the 'out' directory in Release mode
RUN dotnet publish CalendarAssistant/CalendarAssistant.csproj -c Release -o /app/out

# Build the runtime image (using .NET 6.0 runtime image)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /app/out ./

# Expose port 80
EXPOSE 80

# Set the entry point for the application
ENTRYPOINT ["dotnet", "CalendarAssistant.dll"]
