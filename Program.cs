using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;

namespace NethereumSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var web3Instance = new Web3("https://data-seed-prebsc-1-s1.binance.org:8545");
            var contract = GetContractInstance(web3Instance);
            var contractOwner = "0x9FF272ad8BB7c72540Af048e8e7C3741782FFfE9";

            // // get balance native token of account
            // GetAccountBalance(web3Instance, contractOwner).Wait();

            // // get total token supply
            // GetTotalSupply(contract).Wait();

            // // get token name
            // GetTokenName(contract).Wait();

            // // get token balance of account
            // GetTokenBalance(contract, contractOwner).Wait();

            // transfer token from address to another
            Transfer(contract).Wait();
        }

        static dynamic GetContractInstance(Web3 web3Instance) {
            var abi = System.IO.File.ReadAllText(@"./data/abi.txt");
            var contractAddress = "0xef4795044159C131a92E6F8E10d0b1eeb8C0503c";

            var contract = web3Instance.Eth.GetContract(abi, contractAddress);
            return contract;
        }

        // get balance native token of account
        static async Task GetAccountBalance(Web3 web3Instance, string publicKey)
        {
            var balance = await web3Instance.Eth.GetBalance.SendRequestAsync(publicKey);
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            var bnbAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in BNB: {bnbAmount}");
        }
        
        // get token name
        static async Task GetTokenName(dynamic contract) {
            var tokenNameFunction = contract.GetFunction("name");
            var name = await tokenNameFunction.CallAsync<string>();
            Console.WriteLine($"Token name: {name}");
        }

        // get total supply
        static async Task GetTotalSupply(dynamic contract) {
            var totalSupplyFunction = contract.GetFunction("totalSupply");
            var totalSupply = await totalSupplyFunction.CallAsync<BigInteger>();
            Console.WriteLine($"Total supply: {Web3.Convert.FromWei(totalSupply)}");
        }

        // get balance token of account
        static async Task GetTokenBalance(dynamic contract, string account) {
            var balanceOfFunction = contract.GetFunction("balanceOf");
            var amount = await balanceOfFunction.CallAsync<BigInteger>(account);
            Console.WriteLine($"Balance of account {account}: {Web3.Convert.FromWei(amount)} DIEM");
        }

        // transfer token to another account
        static async Task Transfer(dynamic contract) {
            var privateKey = "0x7580e7fb49df1c861f0050fae31c2224c6aba908e116b8da44ee8cd927b990b0";
            var account = new Account(privateKey);
            var senderAddress = account.Address;
            var newAddress = "0xE2EAe88b036Eb5845D6AAe6Da8791396d9118C45";
            var amountToSend = Web3.Convert.ToWei(1); // transfer 1 token. equal token * 10 ** decimals. decimals = 18 same ethereum
            
            // var url = "https://data-seed-prebsc-1-s1.binance.org:8545";
            // var web3 = new Web3(account, url);
            // var abi = System.IO.File.ReadAllText(@"./data/abi.txt");
            // var contractAddress = "0xef4795044159C131a92E6F8E10d0b1eeb8C0503c";
            // var contract = web3.Eth.GetContract(abi, contractAddress);

            try {
                var transferFunction = contract.GetFunction("transfer");
                var gas = await transferFunction.EstimateGasAsync(senderAddress, null, null, newAddress, amountToSend);
                var receiptAmountSend = await transferFunction.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, null, null, newAddress, amountToSend);
                // handle retsult and update database
                // var balance = GetTokenBalance(contract, newAddress).Wait();
                // Console.WriteLine($"Account {newAddress} balance: {balance}");
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

        }
        
    }
}