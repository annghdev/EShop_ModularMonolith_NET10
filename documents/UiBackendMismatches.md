# UI vs Backend Mismatches (Catalog - Products)

Tài liệu này ghi lại các điểm lệch giữa UI (BlazorAdmin) và backend (Catalog) để theo dõi.

## Đã xử lý
- **Publish yêu cầu tối thiểu 1 biến thể**: UI draft đã chặn publish khi chưa có biến thể.
- **Thiếu hiển thị thuộc tính mặc định của danh mục**: UI draft đã hiển thị danh sách mặc định và có nút thêm nhanh các thuộc tính còn thiếu.
- **Variant API dùng productId thay vì slug**: UI service đã dùng productId cho add/update/remove variant.
- **Variant request yêu cầu ProductId/VariantId + MoneyDto**: UI service đã map request đúng format khi gọi backend.
- **HasVariant bị bỏ qua khi thêm thuộc tính**: backend đã lưu giá trị HasVariant vào ProductAttribute.

## Còn tồn tại
- **Update draft không hỗ trợ cập nhật attributes/variants**: API `/api/admin/products/{id}/draft` chỉ cập nhật thông tin cơ bản. Muốn chỉnh thuộc tính/biến thể phải dùng các endpoint riêng.
- **Variant cần đủ attribute values cho mọi thuộc tính**: backend đang validate đủ tất cả attributes, không phân biệt HasVariant. UI hiện đang yêu cầu chọn đầy đủ giá trị cho tất cả thuộc tính để tránh lỗi.
