
public interface IHazardNotifier
{
    void NotifyHazard(string message, string containerNumber);
}
class Program
{
    static void Main(string[] args)
    {
        
        LiquidContainer liquidContainer1 = new LiquidContainer(ContainerSerialNumberGenerator.GenerateSerialNumber("L"), 1000, 200, 100, 5000, true);
        GasContainer gasContainer1 = new GasContainer(ContainerSerialNumberGenerator.GenerateSerialNumber("G"), 1200, 220, 110, 6000, 1.5);
        RefrigeratedContainer refrigeratedContainer1 = new RefrigeratedContainer(ContainerSerialNumberGenerator.GenerateSerialNumber("C"), 1500, 250, 120, 7000, "Banany", 13.3);

        
        ContainerShip containerShip = new ContainerShip(10, 50000, 20);

        
        containerShip.LoadContainer(liquidContainer1);
        containerShip.LoadContainer(gasContainer1);
        containerShip.LoadContainer(refrigeratedContainer1);

        
        liquidContainer1.LoadCargo(2500);
        gasContainer1.LoadCargo(3000);
        refrigeratedContainer1.LoadCargo(4000);

        
        containerShip.PrintContainerList();

        
        try
        {
            liquidContainer1.LoadCargo(3000); 
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        
        LiquidContainer liquidContainer2 = new LiquidContainer(ContainerSerialNumberGenerator.GenerateSerialNumber("L"), 1100, 210, 105, 5500, false);
        containerShip.ReplaceContainer(liquidContainer1.SerialNumber, liquidContainer2);

        
        containerShip.PrintContainerList();

        
        containerShip.UnloadContainer(gasContainer1);

        
        containerShip.PrintContainerList();

        
        RefrigeratedContainer refrigeratedContainer2 = new RefrigeratedContainer(ContainerSerialNumberGenerator.GenerateSerialNumber("C"), 1600, 260, 130, 7500, "Jabłka", 10.5);
        containerShip.LoadContainer(refrigeratedContainer2);

        
        containerShip.PrintContainerList();

        
        gasContainer1.UnloadCargo();

        
        Console.WriteLine($"Kontener gazowy {gasContainer1.SerialNumber}, Ładunek: {gasContainer1.CurrentLoadWeight} kg");
    }
}


public abstract class Container
{
    public string SerialNumber { get; protected set; }
    public double ContainerWeight { get; protected set; }
    public int Height { get; protected set; }
    public int Depth { get; protected set; }
    public int MaxLoadCapacity { get; protected set; }
    public double CurrentLoadWeight { get; protected set; }

    public abstract void LoadCargo(double cargoWeight);

    public Container(string serialNumber, double containerWeight, int height, int depth, int maxLoadCapacity)
    {
        SerialNumber = serialNumber;
        ContainerWeight = containerWeight;
        Height = height;
        Depth = depth;
        MaxLoadCapacity = maxLoadCapacity;
        CurrentLoadWeight = 0;
    }
}


public class LiquidContainer : Container, IHazardNotifier
{
    public bool IsHazardous { get; set; }

    public LiquidContainer(string serialNumber, double containerWeight, int height, int depth, int maxLoadCapacity, bool isHazardous)
        : base(serialNumber, containerWeight, height, depth, maxLoadCapacity)
    {
        IsHazardous = isHazardous;
    }

    public override void LoadCargo(double cargoWeight)
    {
        double maxFillPercentage = IsHazardous ? 0.5 : 0.9;
        if (cargoWeight > MaxLoadCapacity * maxFillPercentage)
        {
            NotifyHazard($"Przekroczono dopuszczalne obciążenie dla kontenera {SerialNumber}", SerialNumber);
            throw new OverflowException("Przekroczono maksymalne obciążenie kontenera.");
        }
        CurrentLoadWeight += cargoWeight;
    }

    public void NotifyHazard(string message, string containerNumber)
    {
        Console.WriteLine($"Ostrzeżenie: {message}, Kontener: {containerNumber}");
    }
}


public class GasContainer : Container, IHazardNotifier
{
    public double PressureInAtmospheres { get; set; }

    public GasContainer(string serialNumber, double containerWeight, int height, int depth, int maxLoadCapacity, double pressure)
        : base(serialNumber, containerWeight, height, depth, maxLoadCapacity)
    {
        PressureInAtmospheres = pressure;
    }

    public override void LoadCargo(double cargoWeight)
    {
        if (cargoWeight > MaxLoadCapacity)
        {
            NotifyHazard($"Przekroczono dopuszczalne obciążenie dla kontenera {SerialNumber}", SerialNumber);
            throw new OverflowException("Przekroczono maksymalne obciążenie kontenera.");
        }
        CurrentLoadWeight += cargoWeight;
    }

    public void UnloadCargo()
    {
        CurrentLoadWeight *= 0.05; 
    }

    public void NotifyHazard(string message, string containerNumber)
    {
        Console.WriteLine($"Ostrzeżenie: {message}, Kontener: {containerNumber}");
    }
}


public class RefrigeratedContainer : Container
{
    public string ProductType { get; set; }
    public double Temperature { get; set; }

    public RefrigeratedContainer(string serialNumber, double containerWeight, int height, int depth, int maxLoadCapacity, string productType, double temperature)
        : base(serialNumber, containerWeight, height, depth, maxLoadCapacity)
    {
        ProductType = productType;
        Temperature = temperature;
    }

    public override void LoadCargo(double cargoWeight)
    {
        if (cargoWeight > MaxLoadCapacity)
        {
            throw new OverflowException("Przekroczono maksymalne obciążenie kontenera.");
        }
        CurrentLoadWeight += cargoWeight;
    }
}


public class ContainerShip
{
    public List<Container> Containers { get; set; }
    public int MaxContainers { get; set; }
    public double MaxTotalWeight { get; set; }
    public int MaxSpeedInKnots { get; set; }

    public ContainerShip(int maxContainers, double maxTotalWeight, int maxSpeed)
    {
        Containers = new List<Container>();
        MaxContainers = maxContainers;
        MaxTotalWeight = maxTotalWeight;
        MaxSpeedInKnots = maxSpeed;
    }

    public void LoadContainer(Container container)
    {
        if (Containers.Count < MaxContainers && CalculateTotalWeight() + container.ContainerWeight <= MaxTotalWeight)
        {
            Containers.Add(container);
        }
        else
        {
            throw new Exception("Nie można załadować więcej kontenerów.");
        }
    }

    public void UnloadContainer(Container container)
    {
        Containers.Remove(container);
    }

    public void ReplaceContainer(string serialNumber, Container newContainer)
    {
        int index = Containers.FindIndex(c => c.SerialNumber == serialNumber);
        if (index != -1)
        {
            Containers[index] = newContainer;
        }
        else
        {
            throw new Exception("Kontener o podanym numerze seryjnym nie istnieje.");
        }
    }

    public void PrintContainerList()
    {
        Console.WriteLine("Lista kontenerów na statku:");
        foreach (var container in Containers)
        {
            Console.WriteLine($"- Numer seryjny: {container.SerialNumber}, Typ: {container.GetType().Name}, Ładunek: {container.CurrentLoadWeight} kg");
        }
    }

    private double CalculateTotalWeight()
    {
        double totalWeight = 0;
        foreach (var container in Containers)
        {
            totalWeight += container.ContainerWeight + container.CurrentLoadWeight;
        }
        return totalWeight;
    }
}
public static class ContainerSerialNumberGenerator
{
    private static int counter = 1;

    public static string GenerateSerialNumber(string containerType)
    {
        return $"KON-{containerType}-{counter++}";
    }
}