## Interact with smart contract

- Step 1: Get contract instance
```cs
var contractInstance = web3.Eth.GetContract(abi, contractAddress);
```

- Step 2: Get function in contract

Access function with name of function in smart contract
```cs
var balanceFunction = contract.GetFunction("balanceOf");
```

- Step 3: Apply method and call function
```cs
var balance = await balanceFunction.CallAsync<int>(newAddress);
```

Để gọi function thì có các phương thức phổ biến sau:

- `CallAsync`: Sử dụng với các hàm chỉ truy vấn các giá trị trên mạng blockchain. Ví dụ: `balanceOf`, `getOwner`
- `SendTransactionAsync` và `SendTransactionAndWaitForReceiptAsync`: Gọi các hàm mà hàm đó thực hiện các thay đổi dữ liệu và tạo 1 giao dịch trên blockchain.included in the next block. Ví dụ: `transfer`, `transferFrom`.
  + `SendTransactionAsync`: Thực hiện 1 chức năng có thay đổi trạng thái trên blockchain và không đợi đến khi nó được xác nhận.
  + `SendTransactionAndWaitForReceiptAsync`: Đợi transaction được khai thác và đóng block.
- `EstimateGasAsync`: Mô phỏng giao dịch để ước lượng số lượng gas cần thiết để thực hiện giao dịch chính thức.