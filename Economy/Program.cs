using System.Collections.Immutable;
using System.Globalization;
using Economy.Serializables;
using Economy.Util;

namespace Economy;

class Program {
    private static List<EconomicData> _data;

    static void Main(string[] args) {
        using var db = new DatabaseService();
        db.CreateTableOrResetTable();

        _data = CsvReader.Read<EconomicData>(
            "Assets/eu_economic_data.csv",
            new() {
                {
                    "YEAR",
                    d => {
                        if (int.TryParse(d, CultureInfo.InvariantCulture, out var year)) {
                            if (year < 2019)
                                return null;
                        }
                        else return null;

                        var dateStr = $"{d.Trim()}-12-31T00:00:00";
                        return ("Year", DateTime.Parse(dateStr));
                    }
                }, {
                    "COUNTRY",
                    s => ("CountryName", s.Trim())
                }, {
                    "GDP_GROWTH",
                    d => {
                        if (double.TryParse(d.Trim().Trim('%').Replace(" ", "").Replace(',', '.'),
                                CultureInfo.InvariantCulture, out var growth))
                            return ("GdpGrowth", growth);
                        return null;
                    }
                }, {
                    "INFLATION",
                    d => {
                        if (double.TryParse(d.Trim().Trim('%').Replace(" ", "").Replace(',', '.'),
                                CultureInfo.InvariantCulture, out var inflation))
                            return ("Inflation", inflation);
                        return null;
                    }
                }
            },
            delimiter: ";"
        );

        db.InsertEconomicData(_data);

        var biggestGrowth = _data.First(d => Math.Abs(d.GdpGrowth - _data.Max(dd => dd.GdpGrowth)) < 0.01);
        var smallestGrowth = _data.First(d => Math.Abs(d.GdpGrowth - _data.Min(dd => dd.GdpGrowth)) < 0.01);

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("GDP NÖVEKEDÉS SZÉLSŐÉRTÉKEK");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("LEGNAGYOBB GDP NÖVEKEDÉS:");
        Console.WriteLine($"  Ország: {biggestGrowth.CountryName}");
        Console.WriteLine($"  Év: {biggestGrowth.Year.Year}");
        Console.WriteLine($"  GDP növekedés: {biggestGrowth.GdpGrowth}%");
        Console.WriteLine("LEGKISEBB GDP NÖVEKEDÉS:");
        Console.WriteLine($"  Ország: {smallestGrowth.CountryName}");
        Console.WriteLine($"  Év: {smallestGrowth.Year.Year}");
        Console.WriteLine($"  GDP növekedés: {smallestGrowth.GdpGrowth}%");
        Console.WriteLine();

        var biggestInflation = _data.First(d => Math.Abs(d.Inflation - _data.Max(dd => dd.Inflation)) < 0.01);
        var smallestInflation = _data.First(d => Math.Abs(d.Inflation - _data.Min(dd => dd.Inflation)) < 0.01);

        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("INFLÁCIÓ SZÉLSŐÉRTÉKEK");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("LEGNAGYOBB INFLÁCIÓ:");
        Console.WriteLine($"  Ország: {biggestInflation.CountryName}");
        Console.WriteLine($"  Év: {biggestInflation.Year.Year}");
        Console.WriteLine($"  Infláció: {biggestInflation.GdpGrowth}%");
        Console.WriteLine("LEGKISEBB INFLÁCIÓ:");
        Console.WriteLine($"  Ország: {smallestInflation.CountryName}");
        Console.WriteLine($"  Év: {smallestInflation.Year.Year}");
        Console.WriteLine($"  Infláció: {smallestInflation.GdpGrowth}%");
        Console.WriteLine();

        var avgInflation = _data
            .GroupBy(d => d.CountryName)
            .Select(d => new AverageInflation() {
                CountryName = d.Key,
                AverageInflationRate = d.Average(dd => dd.Inflation),
            })
            .OrderByDescending(a => a.AverageInflationRate)
            .ToArray();
        
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("ÁTLAGOS ÉVES INFLÁCIÓ (2019-2025)");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();
        foreach (var t in avgInflation) {
            Console.WriteLine($"{t.CountryName}: Átlagos infláció: {Math.Round(t.AverageInflationRate, 2)}%");
        }
        Console.WriteLine();
        
        var euGdpAvg =  _data
            .GroupBy(d => d.Year)
            .Select(d => (d.Key.Year, d.Average(dd => dd.GdpGrowth)))
            .OrderBy(a => a.Item1)
            .Select(a => (a.Item1.ToString(), Math.Round(a.Item2, 2)))
            .ToArray();
        
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("EU ÁTLAGOS GDP NÖVEKEDÉS ÉVENKÉNT");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("Év\tGDP növekedés (%)");
        Console.WriteLine("────────────────────────────────");
        foreach (var t in euGdpAvg) {
            Console.WriteLine($"{t.Item1}\t{t.Item2}");
        }
    }
}