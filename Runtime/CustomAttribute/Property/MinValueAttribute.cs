namespace LF.Runtime
{
    public class MinValueAttribute:CustomPropertyAttributeBase
    {
        public long IntValue;
        public double FloatValue;
        public readonly bool IsIntValue;
        
        public MinValueAttribute(long intValue)
        {
            IntValue = intValue;
            IsIntValue = true;
        }
        
        public MinValueAttribute(double floatValue)
        {
            FloatValue = floatValue;
            IsIntValue = false;
        }
    }
}