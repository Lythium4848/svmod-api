using System.ComponentModel.DataAnnotations;

namespace SVModAPI;

public class Vehicle
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Model { get; set; } = string.Empty;
    
    [Required]
    public string Json { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(32)]
    [MinLength(32)]
    public string Crc { get; set; } = string.Empty;
}

public class AddVehicleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Model { get; set; } = string.Empty;
    [Required]
    public string Json { get; set; } = string.Empty;
}

public class GetVehiclesRequestVehicle
{
    [Required]
    public string Model { get; set; } = string.Empty;
    [Required]
    public string Crc { get; set; } = string.Empty;
}

public class GetVehiclesRequest
{
    public List<GetVehiclesRequestVehicle>? Vehicles { get; set; }
}

public class GetVehiclesResponseVehicle
{
    public string Model { get; set; } = string.Empty;
    public string Json { get; set; } = string.Empty;
}