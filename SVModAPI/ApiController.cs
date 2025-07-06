using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SVModAPI;

[ApiController]
[Route("/")]
public class VehiclesController(ApiDbContext context, ILogger<VehiclesController> logger) : ControllerBase
{
    [HttpPost("add_vehicle")]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> AddVehicle([FromBody] AddVehicleRequest requestBody)
    {
        logger.LogInformation("Received request to add vehicle: {Model}", requestBody.Model);

        // Input validation
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for AddVehicle: {Errors}",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new { error = "Bad Request", message = "Missing required fields or invalid data." });
        }

        try
        {
            JsonDocument.Parse(requestBody.Json);
        }
        catch (JsonException ex)
        {
            logger.LogWarning("Invalid JSON provided for vehicle {Model}: {Message}", requestBody.Model, ex.Message);
            return BadRequest(new
                { error = "Bad Request", message = "Invalid 'json' field. Must be a valid JSON string." });
        }

        var crc = CalculateCrc(requestBody.Json);

        var existingVehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Model == requestBody.Model);
        if (existingVehicle != null)
        {
            existingVehicle.Name = requestBody.Name;
            existingVehicle.Json = requestBody.Json;
            existingVehicle.Crc = crc;

            context.Vehicles.Update(existingVehicle);
            await context.SaveChangesAsync();

            logger.LogInformation("Updated existing vehicle: {Model} (ID: {Id})", existingVehicle.Model,
                existingVehicle.Id);

            return Ok(new { message = "Vehicle updated successfully", id = existingVehicle.Id });
        }
        else
        {
            var newVehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Name = requestBody.Name,
                Model = requestBody.Model,
                Json = requestBody.Json,
                Crc = crc
            };

            context.Vehicles.Add(newVehicle);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully added vehicle: {Model} (ID: {Id})", newVehicle.Model, newVehicle.Id);

            return CreatedAtAction(nameof(AddVehicle), new { id = newVehicle.Id },
                new { message = "Vehicle added successfully", id = newVehicle.Id });
        }
    }

    [HttpPost("get_vehicles")]
    public async Task<IActionResult> GetVehiclesNew([FromBody] GetVehiclesRequest requestBody)
    {
        logger.LogInformation("Received request to get specific vehicles.");

        var responseVehicles = new List<GetVehiclesResponseVehicle>();
        var allServerVehicles = await context.Vehicles.AsNoTracking().ToListAsync();

        if (requestBody.Vehicles == null || requestBody.Vehicles.Count == 0)
            return Ok(responseVehicles);

        var serverVehicleDict = allServerVehicles
            .ToDictionary(v => v.Model, v => v, StringComparer.OrdinalIgnoreCase);
        
        foreach (var clientVehicle in requestBody.Vehicles)
        {
            if (!serverVehicleDict.TryGetValue(clientVehicle.Model, out var serverVehicle)) continue;
            if (string.Equals(serverVehicle.Crc, clientVehicle.Crc, StringComparison.OrdinalIgnoreCase)) continue;

            responseVehicles.Add(new GetVehiclesResponseVehicle
            {
                Model = serverVehicle.Model,
                Json = serverVehicle.Json
            });
            logger.LogInformation(
                "Sending updated vehicle: {Model} (Server CRC: {ServerCrc}, Client CRC: {ClientCrc})",
                serverVehicle.Model, serverVehicle.Crc, clientVehicle.Crc);
        }
        
        return Ok(responseVehicles);
    }

    private static string CalculateCrc(string data)
    {
        var crc32 = new Crc32();
        var dataBytes = Encoding.UTF8.GetBytes(data);

        crc32.Append(dataBytes);
        var hashBytes = crc32.GetCurrentHash();
        var crcValue = BitConverter.ToUInt32(hashBytes, 0);
        return crcValue.ToString();
    }
}