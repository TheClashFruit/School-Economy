namespace Economy.Serializables;

public class EconomicData {
    public string CountryName { get; set; }
    public DateTime Year { get; set; }
    public double GdpGrowth { get; set; }
    public double Inflation { get; set; }

    public override string ToString() {
        return $"{CountryName}; {Year}; {GdpGrowth}; {Inflation}";
    }
}