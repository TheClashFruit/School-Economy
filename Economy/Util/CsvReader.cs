namespace Economy.Util;

public class CsvReader {
    public static List<T> Read<T>(
        string location,
        Dictionary<string, Func<string, (string, object)?>> mapper,
        bool hasHeader = true,
        string delimiter = ","
    ) where T: new() {
        var list = new List<T>();
        
        try {
            using var reader = new StreamReader(location);

            var head = new List<string>();
            
            if (hasHeader) {
                var header = reader.ReadLine()!;
                head.AddRange(header.Trim().Split(delimiter));
            }
            
            while (!reader.EndOfStream) {
                var line = reader.ReadLine()!.Trim().Split(delimiter);
                var data = new T();
                var canAdd = true;
                
                for (var i = 0; i < head.Count; i++) {
                    mapper.TryGetValue(head[i], out var func);
                    var res = func!.Invoke(line[i]);
                    if (res == null) {
                        canAdd = false;
                        continue;
                    }
                    
                    try {
                        var (n, r) = res.Value;
                        
                        var type = typeof(T);
                    
                        type.GetProperty(n)!.SetValue(data, r);
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        canAdd = false;
                    }
                }

                if (!canAdd) continue;
                list.Add(data);
            }
            
            Console.WriteLine($"Sikeresen beolvasva: {list.Count} rekord.");
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        
        return list;
    }
}