namespace LF.Runtime
{
    public class MaxValueAttribute:CustomPropertyAttributeBase
    {
        public long IntValue;
        public double FloatValue;
        public readonly bool IsIntValue;
        
        public MaxValueAttribute(long intValue)
        {
            IntValue = intValue;
            IsIntValue = true;
        }
        
        public MaxValueAttribute(double floatValue)
        {
            FloatValue = floatValue;
            IsIntValue = false;
        }
    }
}