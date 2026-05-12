namespace AwsTest01.Application;
public class TestService : ITestInterface
{
    public string EchoToUpper(string inputStr)
    {
        return inputStr.ToUpper();
    }
}
