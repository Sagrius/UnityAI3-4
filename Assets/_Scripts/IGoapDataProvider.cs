using System;
using System.Collections.Generic;
public interface IGoapDataProvider 
{
    HashSet<KeyValuePair<string, object>> GetWorldState();
}
