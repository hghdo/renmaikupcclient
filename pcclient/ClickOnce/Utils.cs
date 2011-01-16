using System.Deployment.Application;

namespace pcclient.ClickOnce
{
    public class Utils
    {
        public static bool IsApplicationNetworkDeployed
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
