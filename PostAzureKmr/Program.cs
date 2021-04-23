using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using System;

namespace PostAzureKmr
{
    class Program
    {
        static void Main(string[] args)
        {

            #region Credentials
            //Credentials for validate against Azure
            String ClientId = "Put here your number of client";
            String SecretId = "Put here your number of secret";
            String TenantId = "Put here your number of tenant";

            //Creation of credentials
            var credentials = SdkContext.AzureCredentialsFactory
                    .FromServicePrincipal(ClientId, SecretId, TenantId,
                        AzureEnvironment.AzureGlobalCloud);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Creation of credentials: Done");

            //Validation of credentials
            var azure = Azure.Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials).WithDefaultSubscription();
            Console.WriteLine("Validation of credentials: Done");
            #endregion

            #region Creation of services
            try
            {
                #region ResourceGroups
                //Creation of ResourceGroup
                var resourceGroup = azure.ResourceGroups.Define("RG-PostAzureKMR")
                        .WithRegion(Region.USEast).Create();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("ResourceGroup creation: Done");
                #endregion

                #region StorageAccount
                //Creation of storageAccount
                var storageAccount = azure.StorageAccounts.Define("sapostazurekmr")
                        .WithRegion(Region.USEast).WithExistingResourceGroup(resourceGroup)
                            .Create();
                Console.WriteLine("StorageAccount creation: Done");
                #endregion

                #region BlobsContainers
                //Creation BlobService
                var blobService = azure.StorageAccounts.Manager.BlobServices.Define("bspostazurekmr")
                    .WithExistingStorageAccount("RG-PostAzureKMR", "sapostazurekmr").Create();
                Console.WriteLine("BlobService creation: " + blobService + " " + "Done");

                //Creation BlobContainer
                var blobContainer = azure.StorageAccounts.Manager.BlobContainers.DefineContainer("bcpostazurekmr")
                    .WithExistingBlobService("RG-PostAzureKMR", "sapostazurekmr").WithPublicAccess(PublicAccess.Container).Create();
                Console.WriteLine("BlobContainer creation: " + blobContainer + " " + "Done");
                #endregion

                #region WebApps
                //Creation WebApps
                var webApps = azure.WebApps.Define("wapostazurekmr")
                    .WithRegion(Region.USEast).WithExistingResourceGroup(resourceGroup)
                        .WithNewFreeAppServicePlan().Create();
                Console.WriteLine("WebApp creation: Done");
                #endregion

                #region FunctionApp
                //Creation functionApp
                var functionApp = azure.WebApps.Manager.FunctionApps.Define("fapostazurekmr")
                    .WithRegion(Region.USEast).WithExistingResourceGroup(resourceGroup)
                        .WithNewFreeAppServicePlan().WithExistingStorageAccount(storageAccount).Create();
                Console.WriteLine("FunctionApp creation: Done");
                #endregion

                #region KeyVault
                //Creation of KeyVault
                var keyVault = azure.Vaults.Define("kvpostazurekmr").WithRegion(Region.USEast).WithExistingResourceGroup(resourceGroup)
                    .WithEmptyAccessPolicy().Create();
                Console.WriteLine("Azure Key Vault creation: Done");
                #endregion

                #region Virtual Machine
                //Creation of Net
                var network = azure.Networks.Define("network-postazurekmr").WithRegion(Region.USEast)
                     .WithExistingResourceGroup(resourceGroup).WithAddressSpace("10.0.0.0/16")
                        .WithSubnet("subnet-console", "10.0.0.0/24").Create();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("NetWork creation: Done");

                //Creation of IP Public
                var publicIP = azure.PublicIPAddresses.Define("publicip-postazurekmr").WithRegion(Region.USEast)
                    .WithExistingResourceGroup(resourceGroup).WithDynamicIP().Create();
                Console.WriteLine("PublicIP creation: Done");

                //Creation of Network Interface
                var networkInterface = azure.NetworkInterfaces.Define("networkinterface-postazurekmr")
                    .WithRegion(Region.USEast).WithExistingResourceGroup(resourceGroup).WithExistingPrimaryNetwork(network)
                        .WithSubnet("subnet-console").WithPrimaryPrivateIPAddressDynamic().WithExistingPrimaryPublicIPAddress(publicIP).Create();
                Console.WriteLine("NetWork Interface creation: Done");

                //Deploytment of VM
                Console.WriteLine("VM Deploytment...");
                Console.WriteLine("/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/");
                azure.VirtualMachines.Define("VM-CONSOLE").WithRegion(Region.USEast).WithExistingResourceGroup(resourceGroup)
                    .WithExistingPrimaryNetworkInterface(networkInterface).WithPopularLinuxImage(KnownLinuxVirtualMachineImage.UbuntuServer16_04_Lts)
                        .WithRootUsername("userX").WithRootPassword("Tajamar12345").WithComputerName("VM-CONSOLE").WithSize(VirtualMachineSizeTypes.StandardB1s).Create();
                Console.WriteLine("-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/");
                #endregion
            }
            finally
            {
                Console.WriteLine("Successfully created services! ");
                #region Delete RG
                //try
                //{
                //    var rgName = azure.ResourceGroups.GetByName("RG-TestPostAzure");
                //    Console.WriteLine("Deleting Resource Group: "+ rgName);
                //    azure.ResourceGroups.DeleteByName("RG-TestPostAzure");
                //    Console.WriteLine("Deleted Resource Group: " + rgName);
                //}
                //catch (NullReferenceException)
                //{
                //   Console.ForegroundColor = ConsoleColor.Red;
                //   Console.WriteLine("Did not create any resources in Azure. No clean up is necessary");
                //}
                #endregion
            }
            #endregion
        }
    }
}
