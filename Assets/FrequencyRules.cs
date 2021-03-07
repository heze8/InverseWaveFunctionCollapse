using System.Collections.Generic;

public class FrequencyRules
{
    public int[]tileFrequency;

    public FrequencyRules(int numTimes)
    {
        tileFrequency = new int[numTimes];
        for (int i = 0; i < numTimes; i++)
        {
            tileFrequency[i] = 1;
        }
    }
    public int GetRelativeFrequency(int tileIndex)
    {
        return tileFrequency[tileIndex];
    }
    
}
