using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace NethereumSample
{
    class Program
    {
        const int UNLOCK_TIMEOUT = 2 * 60; // 2 minutes
        const int SLEEP_TIME = 5 * 1000; // 5 seconds
        const int MAX_TIMEOUT = 2 * 60 * 1000; // 2 minutes
        const string PROVIDER = "https://data-seed-prebsc-1-s1.binance.org:8545";

        static void Main(string[] args)
        {
            var web3Instance = new Web3(PROVIDER);
            var contract = GetContractInstance(web3Instance);
            var contractOwner = "0x9FF272ad8BB7c72540Af048e8e7C3741782FFfE9";

            // get current blocknumber
            GetBlockNumber(web3Instance).Wait();
            Console.ReadLine();

            // get balance native token of account
            GetAccountBalance(web3Instance, contractOwner).Wait();
            Console.ReadLine();

            // get total token supply
            GetTotalSupply(contract).Wait();
            Console.ReadLine();

            // get token name
            GetTokenName(contract).Wait();
            Console.ReadLine();

            // get token balance of account
            GetTokenBalance(contract, contractOwner).Wait();
            Console.ReadLine();

            // transfer token from address to another
            Transfer().Wait();
            Console.ReadLine();
        }

        static dynamic GetContractInstance(Web3 web3Instance) {
            var abi = System.IO.File.ReadAllText(@"./data/abi.txt");
            var contractAddress = "0xef4795044159C131a92E6F8E10d0b1eeb8C0503c";

            var contract = web3Instance.Eth.GetContract(abi, contractAddress);
            return contract;
        }

        // get current block number
        static async Task GetBlockNumber(Web3 web3Instance)
        {
        	var latestBlockNumber = await web3Instance.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        	Console.WriteLine($"Latest Block Number is: {latestBlockNumber}");
        }

        // get balance native token of account
        static async Task<Decimal> GetAccountBalance(Web3 web3Instance, string publicKey)
        {
            var balance = await web3Instance.Eth.GetBalance.SendRequestAsync(publicKey);
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            var bnbAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in BNB: {bnbAmount}");
            return bnbAmount;
        }
        
        // get token name
        static async Task<String> GetTokenName(dynamic contract) {
            var tokenNameFunction = contract.GetFunction("name");
            var name = await tokenNameFunction.CallAsync<string>();
            Console.WriteLine($"Token name: {name}");
            return name;
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
        static async Task Transfer() {
            var privateKey = "02841b83aabf07659f4d177ab6ed2c0a30c5ffcec5fd8ebdfcd118976b10350d"; // replace with your private key
            var account = new Account(privateKey, 97);
            var web3 = new Web3(account, PROVIDER);
            web3.TransactionManager.UseLegacyAsDefault = true; // fix error: transaction type not supported: eth_sendRawTransaction // https://stackoverflow.com/questions/68855027/nethereuem-sendtransactionasync-from-my-c-sharp-web-api-fails-with-transaction-t
            
            var abi = System.IO.File.ReadAllText(@"./data/abi.txt");
            var contractAddress = "0xef4795044159C131a92E6F8E10d0b1eeb8C0503c";
            var contract = web3.Eth.GetContract(abi, contractAddress);

            var senderAddress = account.Address;
            var newAddress = "0xE2EAe88b036Eb5845D6AAe6Da8791396d9118C45";
            var amountToSend = Web3.Convert.ToWei(1); // transfer 1 token. equal token * 10 ** decimals. decimals = 18 same ethereum
            var transferFunction = contract.GetFunction("transfer");

            // gas estimated, in wei
            var gas = await transferFunction.EstimateGasAsync(senderAddress, null, null, newAddress, amountToSend);
            // use `SendTransactionAsync` method
            var txHash1 = await transferFunction.SendTransactionAsync(senderAddress, gas, null, null, newAddress, amountToSend);
            Console.WriteLine("txHash1:\t" + txHash1.ToString());
            var txReceipt1 = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash1);
            int timeoutCount = 0;
            while (txReceipt1 == null && timeoutCount < MAX_TIMEOUT)
            {
                Console.WriteLine("Sleeping...");
                Thread.Sleep(SLEEP_TIME);
                txReceipt1 = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash1);
                timeoutCount += SLEEP_TIME;
            }
            Console.WriteLine("timeoutCount:\t" + timeoutCount.ToString());

            // use `SendTransactionAndWaitForReceiptAsync` method
            var txReceipt2 = await transferFunction.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, null, null, newAddress, amountToSend);
            Console.WriteLine("txReceipt2:\t" + txReceipt2.TransactionHash.ToString());
            Console.WriteLine("txReceipt2 > CumulativeGasUsed:\t" + txReceipt2.CumulativeGasUsed.Value.ToString());
            
            // handle retsult and update database...
        }

        static public async Task WaitForTxReceiptExample(Web3 web3, string txHash) {
            Console.WriteLine("WaitForTxReceiptExample:");

            int timeoutCount = 0;
            var txReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            while (txReceipt == null && timeoutCount < MAX_TIMEOUT)
            {
                Console.WriteLine("Sleeping...");
                Thread.Sleep(SLEEP_TIME);
                txReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
                timeoutCount += SLEEP_TIME;
            }
            Console.WriteLine("timeoutCount " + timeoutCount.ToString());
            LastTxReceipt = txReceipt;
        }
        
    }
}