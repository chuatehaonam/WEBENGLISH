using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TerminalShop
{
    // Enums cho các constant values
    public enum PaymentMethod
    {
        Cash = 1,
        CreditCard = 2,
        DigitalWallet = 3
    }

    public enum ProductSize
    {
        XS, S, M, L, XL, XXL
    }

    public enum MenuOption
    {
        SearchProducts = 1,
        AddToCart = 2,
        ViewCart = 3,
        Checkout = 4,
        AddProduct = 5,
        EditProduct = 6,
        ViewAllProducts = 7,
        RemoveFromCart = 8,
        EditCartQuantity = 9,
        ClearCart = 10,
        Exit = 0
    }

    // Product class với properties và validation
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public ProductSize Size { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Category { get; set; }

        public Product(int id, string name, decimal price, string description, ProductSize size, int stock, string category = "General")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tên sản phẩm không được để trống");
            if (price <= 0)
                throw new ArgumentException("Giá sản phẩm phải lớn hơn 0");
            if (stock < 0)
                throw new ArgumentException("Số lượng tồn kho không được âm");

            Id = id;
            Name = name;
            Price = price;
            Description = description;
            Size = size;
            Stock = stock;
            Category = category;
            CreatedDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Id:D3}] {Name} - {Price:C} | Size: {Size} | Stock: {Stock} | Category: {Category}";
        }
    }

    // CartItem class
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }

        public CartItem(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0");

            Product = product;
            Quantity = quantity;
            AddedDate = DateTime.Now;
        }

        public decimal TotalPrice => Product.Price * Quantity;

        public override string ToString()
        {
            return $"• {Product.Name} x {Quantity} = {TotalPrice:C} (Size: {Product.Size})";
        }
    }

    // Shopping Cart class
    public class ShoppingCart
    {
        private List<CartItem> _items = new List<CartItem>();
        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
        public DateTime LastUpdated { get; private set; }

        public void AddItem(Product product, int quantity)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (quantity <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0");
            if (quantity > product.Stock) throw new InvalidOperationException("Không đủ hàng trong kho");

            var existingItem = _items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > product.Stock)
                    throw new InvalidOperationException("Không đủ hàng trong kho");
                existingItem.Quantity += quantity;
            }
            else
            {
                _items.Add(new CartItem(product, quantity));
            }

            LastUpdated = DateTime.Now;
        }

        public bool RemoveItem(int productId)
        {
            var removed = _items.RemoveAll(i => i.Product.Id == productId) > 0;
            if (removed) LastUpdated = DateTime.Now;
            return removed;
        }

        public bool UpdateQuantity(int productId, int newQuantity)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                if (newQuantity <= 0)
                {
                    return RemoveItem(productId);
                }
                else
                {
                    item.Quantity = newQuantity;
                    LastUpdated = DateTime.Now;
                    return true;
                }
            }
            return false;
        }

        public void ClearCart()
        {
            _items.Clear();
            LastUpdated = DateTime.Now;
        }

        public decimal GetTotal() => _items.Sum(i => i.TotalPrice);

        public int GetTotalItems() => _items.Sum(i => i.Quantity);

        public bool IsEmpty => !_items.Any();
    }

    // UI Helper class cho console formatting
    public static class ConsoleUI
    {
        public static void WriteHeader(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', 60) + "╗");
            Console.WriteLine($"║{title.PadLeft((60 + title.Length) / 2).PadRight(60)}║");
            Console.WriteLine("╚" + new string('═', 60) + "╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ {message}");
            Console.ResetColor();
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {message}");
            Console.ResetColor();
        }

        public static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  {message}");
            Console.ResetColor();
        }

        public static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"ℹ️  {message}");
            Console.ResetColor();
        }

        public static void WriteSeparator()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('─', 60));
            Console.ResetColor();
        }

        public static void PressAnyKeyToContinue()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nNhấn phím bất kỳ để tiếp tục...");
            Console.ResetColor();
            Console.ReadKey();
        }

        public static async Task ShowLoadingAnimation(string message, int duration = 2000)
        {
            Console.Write($"{message} ");
            var chars = "|/-\\";
            var end = DateTime.Now.AddMilliseconds(duration);
            
            while (DateTime.Now < end)
            {
                foreach (var c in chars)
                {
                    Console.Write(c);
                    await Task.Delay(100);
                    Console.Write("\b");
                    if (DateTime.Now >= end) break;
                }
            }
            Console.WriteLine("✅");
        }
    }

    // Main Shop System class
    public class AdvancedShopSystem
    {
        private List<Product> _products = new List<Product>();
        private ShoppingCart _cart = new ShoppingCart();
        private int _nextProductId = 1;

        public AdvancedShopSystem()
        {
            InitializeDefaultProducts();
        }

        private void InitializeDefaultProducts()
        {
            var defaultProducts = new[]
            {
                new Product(_nextProductId++, "T-Shirt Premium Cotton", 15.99m, "100% cotton premium t-shirt", ProductSize.M, 50, "Clothing"),
                new Product(_nextProductId++, "Polo Shirt Classic", 24.99m, "Classic polo shirt for business casual", ProductSize.L, 30, "Clothing"),
                new Product(_nextProductId++, "Hoodie Urban Black", 39.99m, "Urban style black hoodie with kangaroo pocket", ProductSize.XL, 20, "Clothing"),
                new Product(_nextProductId++, "Sweater Wool Blend", 45.50m, "Premium wool blend sweater", ProductSize.L, 15, "Clothing"),
                new Product(_nextProductId++, "Tank Top Summer", 12.99m, "Lightweight summer tank top", ProductSize.S, 40, "Clothing"),
                new Product(_nextProductId++, "Jeans Slim Fit Premium", 55.99m, "Premium denim slim fit jeans", ProductSize.M, 35, "Pants"),
                new Product(_nextProductId++, "Chino Pants Business", 42.99m, "Professional chino pants", ProductSize.L, 25, "Pants"),
                new Product(_nextProductId++, "Shorts Khaki Summer", 22.99m, "Lightweight khaki shorts perfect for summer", ProductSize.M, 40, "Pants"),
                new Product(_nextProductId++, "Jogger Pants Comfort", 35.50m, "Ultra comfortable jogger pants", ProductSize.XL, 30, "Pants"),
                new Product(_nextProductId++, "Dress Pants Formal", 65.99m, "Formal dress pants for business", ProductSize.L, 10, "Pants")
            };

            _products.AddRange(defaultProducts);
        }

        public void DisplayMainMenu()
        {
            ConsoleUI.WriteHeader("🛍️  TERMINAL SHOP - HỆ THỐNG BÁN HÀNG CHUYÊN NGHIỆP");
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│                    MENU CHÍNH                          │");
            Console.WriteLine("├─────────────────────────────────────────────────────────┤");
            Console.WriteLine("│  1. 🔍 Tìm kiếm sản phẩm                               │");
            Console.WriteLine("│  2. 🛒 Thêm sản phẩm vào giỏ hàng                      │");
            Console.WriteLine("│  3. 👀 Xem giỏ hàng                                    │");
            Console.WriteLine("│  4. 💳 Thanh toán                                      │");
            Console.WriteLine("│  5. ➕ Thêm sản phẩm mới (Admin)                      │");
            Console.WriteLine("│  6. ✏️  Chỉnh sửa sản phẩm (Admin)                     │");
            Console.WriteLine("│  7. 📋 Xem tất cả sản phẩm                             │");
            Console.WriteLine("│  8. 🗑️  Xóa sản phẩm khỏi giỏ hàng                     │");
            Console.WriteLine("│  9. ✏️  Chỉnh sửa số lượng trong giỏ hàng              │");
            Console.WriteLine("│ 10. 🧹 Làm trống giỏ hàng                              │");
            Console.WriteLine("│  0. 🚪 Thoát                                           │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");
            Console.ResetColor();

            if (!_cart.IsEmpty)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n🛒 Giỏ hàng: {_cart.GetTotalItems()} sản phẩm - Tổng: {_cart.GetTotal():C}");
                Console.ResetColor();
            }

            Console.Write("\n➤ Vui lòng chọn một tùy chọn: ");
        }

        public async Task SearchProducts()
        {
            ConsoleUI.WriteHeader("🔍 TÌM KIẾM SẢN PHẨM");
            
            Console.Write("Nhập từ khóa tìm kiếm: ");
            string keyword = Console.ReadLine()?.ToLower();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ConsoleUI.WriteWarning("Từ khóa tìm kiếm không được để trống!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            await ConsoleUI.ShowLoadingAnimation("Đang tìm kiếm", 1000);

            var results = _products.Where(p => 
                p.Name.ToLower().Contains(keyword) || 
                p.Description.ToLower().Contains(keyword) ||
                p.Category.ToLower().Contains(keyword)).ToList();

            if (!results.Any())
            {
                ConsoleUI.WriteError("Không tìm thấy sản phẩm nào phù hợp!");
            }
            else
            {
                ConsoleUI.WriteSuccess($"Tìm thấy {results.Count} sản phẩm:");
                ConsoleUI.WriteSeparator();
                
                foreach (var product in results)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(product.ToString());
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"   📝 {product.Description}");
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }

            ConsoleUI.PressAnyKeyToContinue();
        }

        public async Task AddProductToCart()
        {
            ConsoleUI.WriteHeader("🛒 THÊM SẢN PHẨM VÀO GIỎ HÀNG");

            if (!_products.Any())
            {
                ConsoleUI.WriteError("Không có sản phẩm nào trong cửa hàng!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            Console.Write("Nhập ID sản phẩm: ");
            if (!int.TryParse(Console.ReadLine(), out int productId))
            {
                ConsoleUI.WriteError("ID sản phẩm không hợp lệ!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                ConsoleUI.WriteError("Không tìm thấy sản phẩm!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine($"\n📦 Sản phẩm được chọn: {product}");
            Console.Write("Nhập số lượng: ");
            
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
            {
                ConsoleUI.WriteError("Số lượng không hợp lệ!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            try
            {
                await ConsoleUI.ShowLoadingAnimation("Đang thêm vào giỏ hàng", 800);
                _cart.AddItem(product, quantity);
                ConsoleUI.WriteSuccess($"Đã thêm {quantity} {product.Name} vào giỏ hàng!");
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteError(ex.Message);
            }

            ConsoleUI.PressAnyKeyToContinue();
        }

        public void DisplayCart()
        {
            ConsoleUI.WriteHeader("🛒 GIỎ HÀNG CỦA BẠN");

            if (_cart.IsEmpty)
            {
                ConsoleUI.WriteWarning("Giỏ hàng của bạn đang trống!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine($"📅 Cập nhật lần cuối: {_cart.LastUpdated:dd/MM/yyyy HH:mm:ss}");
            ConsoleUI.WriteSeparator();

            foreach (var item in _cart.Items)
            {
                Console.WriteLine(item.ToString());
            }

            ConsoleUI.WriteSeparator();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"💰 Tổng số sản phẩm: {_cart.GetTotalItems()}");
            Console.WriteLine($"💵 Tổng tiền: {_cart.GetTotal():C}");
            Console.ResetColor();

            ConsoleUI.PressAnyKeyToContinue();
        }

        public async Task ProcessCheckout()
        {
            ConsoleUI.WriteHeader("💳 THANH TOÁN");

            if (_cart.IsEmpty)
            {
                ConsoleUI.WriteWarning("Giỏ hàng của bạn đang trống!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            // Display cart summary
            Console.WriteLine("📋 Tóm tắt đơn hàng:");
            ConsoleUI.WriteSeparator();
            foreach (var item in _cart.Items)
            {
                Console.WriteLine(item.ToString());
            }
            ConsoleUI.WriteSeparator();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"💵 Tổng tiền: {_cart.GetTotal():C}");
            Console.ResetColor();

            // Get shipping information
            Console.Write("\n👤 Họ và tên người nhận: ");
            string recipientName = Console.ReadLine();
            
            Console.Write("📍 Địa chỉ giao hàng: ");
            string shippingAddress = Console.ReadLine();

            Console.Write("📞 Số điện thoại: ");
            string phoneNumber = Console.ReadLine();

            // Payment method selection
            Console.WriteLine("\n💳 Chọn phương thức thanh toán:");
            Console.WriteLine("1. 💵 Tiền mặt (COD)");
            Console.WriteLine("2. 💳 Thẻ tín dụng");
            Console.WriteLine("3. 📱 Ví điện tử");
            
            Console.Write("Lựa chọn: ");
            if (!int.TryParse(Console.ReadLine(), out int paymentChoice) || 
                !Enum.IsDefined(typeof(PaymentMethod), paymentChoice))
            {
                ConsoleUI.WriteError("Phương thức thanh toán không hợp lệ!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            var paymentMethod = (PaymentMethod)paymentChoice;

            // Process payment
            await ConsoleUI.ShowLoadingAnimation("Đang xử lý thanh toán", 3000);

            // Simulate payment processing
            var random = new Random();
            bool paymentSuccess = random.Next(1, 101) <= 95; // 95% thành công

            if (paymentSuccess)
            {
                var orderId = $"ORD{DateTime.Now:yyyyMMddHHmmss}";
                
                ConsoleUI.WriteSuccess("Thanh toán thành công!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔═══════════════════════════════════════╗");
                Console.WriteLine("║            HÓA ĐON BÁN HÀNG           ║");
                Console.WriteLine("╠═══════════════════════════════════════╣");
                Console.WriteLine($"║ Mã đơn hàng: {orderId.PadRight(20)} ║");
                Console.WriteLine($"║ Ngày: {DateTime.Now:dd/MM/yyyy HH:mm:ss}          ║");
                Console.WriteLine($"║ Khách hàng: {recipientName.PadRight(22)} ║");
                Console.WriteLine($"║ Địa chỉ: {shippingAddress.PadRight(25)} ║");
                Console.WriteLine($"║ SĐT: {phoneNumber.PadRight(29)} ║");
                Console.WriteLine($"║ Thanh toán: {paymentMethod.ToString().PadRight(22)} ║");
                Console.WriteLine($"║ Tổng tiền: {_cart.GetTotal():C}".PadRight(38) + " ║");
                Console.WriteLine("╚═══════════════════════════════════════╝");
                Console.ResetColor();

                // Update product stock
                foreach (var item in _cart.Items)
                {
                    item.Product.Stock -= item.Quantity;
                }

                _cart.ClearCart();
                ConsoleUI.WriteInfo("Đơn hàng sẽ được giao trong 2-3 ngày làm việc.");
            }
            else
            {
                ConsoleUI.WriteError("Thanh toán thất bại! Vui lòng thử lại.");
            }

            ConsoleUI.PressAnyKeyToContinue();
        }

        public void AddNewProduct()
        {
            ConsoleUI.WriteHeader("➕ THÊM SẢN PHẨM MỚI");

            try
            {
                Console.Write("Tên sản phẩm: ");
                string name = Console.ReadLine();

                Console.Write("Giá bán: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price))
                {
                    ConsoleUI.WriteError("Giá bán không hợp lệ!");
                    ConsoleUI.PressAnyKeyToContinue();
                    return;
                }

                Console.Write("Mô tả: ");
                string description = Console.ReadLine();

                Console.Write("Danh mục: ");
                string category = Console.ReadLine();

                Console.WriteLine("Kích thước (0=XS, 1=S, 2=M, 3=L, 4=XL, 5=XXL): ");
                if (!int.TryParse(Console.ReadLine(), out int sizeIndex) || 
                    !Enum.IsDefined(typeof(ProductSize), sizeIndex))
                {
                    ConsoleUI.WriteError("Kích thước không hợp lệ!");
                    ConsoleUI.PressAnyKeyToContinue();
                    return;
                }

                Console.Write("Số lượng tồn kho: ");
                if (!int.TryParse(Console.ReadLine(), out int stock))
                {
                    ConsoleUI.WriteError("Số lượng tồn kho không hợp lệ!");
                    ConsoleUI.PressAnyKeyToContinue();
                    return;
                }

                var product = new Product(_nextProductId++, name, price, description, 
                    (ProductSize)sizeIndex, stock, category);
                _products.Add(product);

                ConsoleUI.WriteSuccess("Đã thêm sản phẩm thành công!");
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteError($"Lỗi: {ex.Message}");
            }

            ConsoleUI.PressAnyKeyToContinue();
        }

        public void EditProduct()
        {
            ConsoleUI.WriteHeader("✏️ CHỈNH SỬA SẢN PHẨM");

            Console.Write("Nhập ID sản phẩm cần chỉnh sửa: ");
            if (!int.TryParse(Console.ReadLine(), out int productId))
            {
                ConsoleUI.WriteError("ID sản phẩm không hợp lệ!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                ConsoleUI.WriteError("Không tìm thấy sản phẩm!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine($"\n📦 Sản phẩm hiện tại: {product}");
            Console.WriteLine("Nhấn Enter để giữ nguyên giá trị hiện tại\n");

            Console.Write($"Tên mới ({product.Name}): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
                product.Name = newName;

            Console.Write($"Giá mới ({product.Price}): ");
            string priceStr = Console.ReadLine();
            if (decimal.TryParse(priceStr, out decimal newPrice) && newPrice > 0)
                product.Price = newPrice;

            Console.Write($"Mô tả mới ({product.Description}): ");
            string newDescription = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newDescription))
                product.Description = newDescription;

            Console.Write($"Số lượng mới ({product.Stock}): ");
            string stockStr = Console.ReadLine();
            if (int.TryParse(stockStr, out int newStock) && newStock >= 0)
                product.Stock = newStock;

            ConsoleUI.WriteSuccess("Cập nhật sản phẩm thành công!");
            ConsoleUI.PressAnyKeyToContinue();
        }

        public void ViewAllProducts()
        {
            ConsoleUI.WriteHeader("📋 TẤT CẢ SẢN PHẨM");

            if (!_products.Any())
            {
                ConsoleUI.WriteWarning("Không có sản phẩm nào trong cửa hàng!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            var groupedProducts = _products.GroupBy(p => p.Category);

            foreach (var group in groupedProducts)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n🏷️  {group.Key.ToUpper()}");
                Console.ResetColor();
                ConsoleUI.WriteSeparator();

                foreach (var product in group)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(product.ToString());
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"   📝 {product.Description}");
                    Console.ResetColor();
                }
            }

            Console.WriteLine($"\n📊 Tổng số sản phẩm: {_products.Count}");
            ConsoleUI.PressAnyKeyToContinue();
        }

        public void RemoveFromCart()
        {
            ConsoleUI.WriteHeader("🗑️ XÓA SẢN PHẨM KHỎI GIỎ HÀNG");

            if (_cart.IsEmpty)
            {
                ConsoleUI.WriteWarning("Giỏ hàng của bạn đang trống!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            // Hiển thị giỏ hàng với ID rõ ràng
            Console.WriteLine("📋 Danh sách sản phẩm trong giỏ hàng:");
            ConsoleUI.WriteSeparator();
            foreach (var item in _cart.Items)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"🆔 ID: {item.Product.Id} | {item.Product.Name} x {item.Quantity} = {item.TotalPrice:C}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   Size: {item.Product.Size} | Đã thêm: {item.AddedDate:dd/MM/yyyy HH:mm}");
                Console.ResetColor();
                Console.WriteLine();
            }
            ConsoleUI.WriteSeparator();

            Console.Write("🔢 Nhập ID sản phẩm cần xóa: ");
            if (!int.TryParse(Console.ReadLine(), out int productId))
            {
                ConsoleUI.WriteError("ID sản phẩm không hợp lệ!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            // Tìm sản phẩm trong giỏ hàng
            var cartItem = _cart.Items.FirstOrDefault(item => item.Product.Id == productId);
            if (cartItem == null)
            {
                ConsoleUI.WriteError($"Không tìm thấy sản phẩm có ID {productId} trong giỏ hàng!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            // Hiển thị thông tin sản phẩm sẽ xóa
            Console.WriteLine($"\n📦 Sản phẩm sẽ bị xóa:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   • {cartItem.Product.Name}");
            Console.WriteLine($"   • Số lượng hiện tại: {cartItem.Quantity}");
            Console.WriteLine($"   • Tổng giá: {cartItem.TotalPrice:C}");
            Console.ResetColor();

            // Tùy chọn xóa
            Console.WriteLine("\n🔧 Chọn hành động:");
            Console.WriteLine("1. 🗑️  Xóa hoàn toàn sản phẩm");
            Console.WriteLine($"2. ➖ Giảm số lượng (hiện tại: {cartItem.Quantity})");
            Console.WriteLine("0. ❌ Hủy bỏ");
            
            Console.Write("Lựa chọn: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                ConsoleUI.WriteError("Lựa chọn không hợp lệ!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            switch (choice)
            {
                case 1: // Xóa hoàn toàn
                    Console.Write($"⚠️  Bạn có chắc chắn muốn xóa hoàn toàn '{cartItem.Product.Name}' khỏi giỏ hàng? (y/n): ");
                    string confirmDelete = Console.ReadLine()?.ToLower();
                    if (confirmDelete == "y" || confirmDelete == "yes")
                    {
                        if (_cart.RemoveItem(productId))
                        {
                            ConsoleUI.WriteSuccess($"✅ Đã xóa '{cartItem.Product.Name}' khỏi giỏ hàng!");
                        }
                        else
                        {
                            ConsoleUI.WriteError("❌ Có lỗi xảy ra khi xóa sản phẩm!");
                        }
                    }
                    else
                    {
                        ConsoleUI.WriteInfo("ℹ️  Hủy thao tác xóa.");
                    }
                    break;

                case 2: // Giảm số lượng
                    Console.Write($"Nhập số lượng cần giảm (1-{cartItem.Quantity}): ");
                    if (int.TryParse(Console.ReadLine(), out int reduceQty) && 
                        reduceQty > 0 && reduceQty <= cartItem.Quantity)
                    {
                        if (reduceQty == cartItem.Quantity)
                        {
                            // Nếu giảm hết thì xóa luôn
                            _cart.RemoveItem(productId);
                            ConsoleUI.WriteSuccess($"✅ Đã xóa hoàn toàn '{cartItem.Product.Name}' khỏi giỏ hàng!");
                        }
                        else
                        {
                            // Giảm số lượng
                            int newQuantity = cartItem.Quantity - reduceQty;
                            _cart.UpdateQuantity(productId, newQuantity);
                            ConsoleUI.WriteSuccess($"✅ Đã giảm {reduceQty} '{cartItem.Product.Name}'. Còn lại: {newQuantity}");
                        }
                    }
                    else
                    {
                        ConsoleUI.WriteError("❌ Số lượng không hợp lệ!");
                    }
                    break;

                case 0: // Hủy bỏ
                    ConsoleUI.WriteInfo("ℹ️  Hủy thao tác.");
                    break;

                default:
                    ConsoleUI.WriteError("❌ Lựa chọn không hợp lệ!");
                    break;
            }

            ConsoleUI.PressAnyKeyToContinue();
        }

        public void EditCartQuantity()
        {
            ConsoleUI.WriteHeader("✏️ CHỈNH SỬA SỐ LƯỢNG SẢN PHẨM");

            if (_cart.IsEmpty)
            {
                ConsoleUI.WriteWarning("Giỏ hàng của bạn đang trống!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            // Hiển thị giỏ hàng với ID rõ ràng
            Console.WriteLine("📋 Danh sách sản phẩm trong giỏ hàng:");
            ConsoleUI.WriteSeparator();
            foreach (var item in _cart.Items)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"🆔 ID: {item.Product.Id} | {item.Product.Name} x {item.Quantity} = {item.TotalPrice:C}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   Size: {item.Product.Size} | Có sẵn: {item.Product.Stock + item.Quantity} | Đã thêm: {item.AddedDate:dd/MM/yyyy HH:mm}");
                Console.ResetColor();
                Console.WriteLine();
            }
            ConsoleUI.WriteSeparator();

            Console.Write("🔢 Nhập ID sản phẩm cần chỉnh sửa: ");
            if (!int.TryParse(Console.ReadLine(), out int productId))
            {
                ConsoleUI.WriteError("ID sản phẩm không hợp lệ!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            // Tìm sản phẩm trong giỏ hàng
            var cartItem = _cart.Items.FirstOrDefault(item => item.Product.Id == productId);
            if (cartItem == null)
            {
                ConsoleUI.WriteError($"Không tìm thấy sản phẩm có ID {productId} trong giỏ hàng!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            // Hiển thị thông tin sản phẩm
            Console.WriteLine($"\n📦 Thông tin sản phẩm:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   • Tên: {cartItem.Product.Name}");
            Console.WriteLine($"   • Số lượng hiện tại: {cartItem.Quantity}");
            Console.WriteLine($"   • Số lượng có sẵn: {cartItem.Product.Stock + cartItem.Quantity}");
            Console.WriteLine($"   • Giá: {cartItem.Product.Price:C} / sản phẩm");
            Console.WriteLine($"   • Tổng giá hiện tại: {cartItem.TotalPrice:C}");
            Console.ResetColor();

            int maxAvailable = cartItem.Product.Stock + cartItem.Quantity;
            Console.Write($"\n🔢 Nhập số lượng mới (1-{maxAvailable}): ");
            
            if (!int.TryParse(Console.ReadLine(), out int newQuantity) || newQuantity <= 0)
            {
                ConsoleUI.WriteError("Số lượng không hợp lệ!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            if (newQuantity > maxAvailable)
            {
                ConsoleUI.WriteError($"Số lượng vượt quá số lượng có sẵn ({maxAvailable})!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            // Xác nhận thay đổi
            decimal newTotalPrice = cartItem.Product.Price * newQuantity;
            Console.WriteLine($"\n📊 Thay đổi:");
            Console.WriteLine($"   • Số lượng: {cartItem.Quantity} → {newQuantity}");
            Console.WriteLine($"   • Tổng giá: {cartItem.TotalPrice:C} → {newTotalPrice:C}");
            
            Console.Write("✅ Bạn có chắc chắn muốn thay đổi? (y/n): ");
            string confirm = Console.ReadLine()?.ToLower();
            
            if (confirm == "y" || confirm == "yes")
            {
                if (_cart.UpdateQuantity(productId, newQuantity))
                {
                    ConsoleUI.WriteSuccess($"✅ Đã cập nhật số lượng '{cartItem.Product.Name}' thành {newQuantity}!");
                    
                    // Cập nhật stock sản phẩm
                    int stockChange = cartItem.Quantity - newQuantity;
                    cartItem.Product.Stock += stockChange;
                }
                else
                {
                    ConsoleUI.WriteError("❌ Có lỗi xảy ra khi cập nhật!");
                }
            }
            else
            {
                ConsoleUI.WriteInfo("ℹ️  Hủy thao tác chỉnh sửa.");
            }

            ConsoleUI.PressAnyKeyToContinue();
        }

        public void ClearCart()
        {
            ConsoleUI.WriteHeader("🧹 LÀM TRỐNG GIỎ HÀNG");

            if (_cart.IsEmpty)
            {
                ConsoleUI.WriteWarning("Giỏ hàng của bạn đã trống!");
                ConsoleUI.PressAnyKeyToContinue();
                return;
            }

            Console.Write("Bạn có chắc chắn muốn xóa tất cả sản phẩm? (y/n): ");
            string confirmation = Console.ReadLine()?.ToLower();

            if (confirmation == "y" || confirmation == "yes")
            {
                _cart.ClearCart();
                ConsoleUI.WriteSuccess("Đã làm trống giỏ hàng!");
            }
            else
            {
                ConsoleUI.WriteInfo("Hủy thao tác.");
            }

            ConsoleUI.PressAnyKeyToContinue();
        }

        public async Task<MenuOption> GetMenuChoice()
        {
            DisplayMainMenu();
            
            if (int.TryParse(Console.ReadLine(), out int choice) && 
                Enum.IsDefined(typeof(MenuOption), choice))
            {
                return (MenuOption)choice;
            }

            ConsoleUI.WriteError("Lựa chọn không hợp lệ! Vui lòng thử lại.");
            await Task.Delay(1500);
            return MenuOption.Exit; // Return invalid to loop again
        }

        public async Task RunMainLoop()
        {
            ConsoleUI.WriteHeader("🎉 CHÀO MỪNG ĐẾN VỚI TERMINAL SHOP!");
            ConsoleUI.WriteInfo("Hệ thống bán hàng chuyên nghiệp trên terminal");
            await Task.Delay(2000);

            while (true)
            {
                try
                {
                    var choice = await GetMenuChoice();

                    switch (choice)
                    {
                        case MenuOption.SearchProducts:
                            await SearchProducts();
                            break;
                        case MenuOption.AddToCart:
                            await AddProductToCart();
                            break;
                        case MenuOption.ViewCart:
                            DisplayCart();
                            break;
                        case MenuOption.Checkout:
                            await ProcessCheckout();
                            break;
                        case MenuOption.AddProduct:
                            AddNewProduct();
                            break;
                        case MenuOption.EditProduct:
                            EditProduct();
                            break;
                        case MenuOption.ViewAllProducts:
                            ViewAllProducts();
                            break;
                        case MenuOption.RemoveFromCart:
                            RemoveFromCart();
                            break;
                        case MenuOption.EditCartQuantity:
                            EditCartQuantity();
                            break;
                        case MenuOption.ClearCart:
                            ClearCart();
                            break;
                        case MenuOption.Exit:
                            ConsoleUI.WriteHeader("👋 CẢM ơN BẠN ĐÃ SỬ DỤNG TERMINAL SHOP!");
                            ConsoleUI.WriteInfo("Hẹn gặp lại bạn lần sau!");
                            await Task.Delay(2000);
                            return;
                        default:
                            ConsoleUI.WriteError("Lựa chọn không hợp lệ!");
                            await Task.Delay(1000);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUI.WriteError($"Đã xảy ra lỗi: {ex.Message}");
                    ConsoleUI.PressAnyKeyToContinue();
                }
            }
        }
    }

    // Main Program class
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Terminal Shop - Hệ thống bán hàng chuyên nghiệp";

            try
            {
                var shop = new AdvancedShopSystem();
                await shop.RunMainLoop();
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteError($"Lỗi hệ thống: {ex.Message}");
                ConsoleUI.PressAnyKeyToContinue();
            }
        }
    }
} 