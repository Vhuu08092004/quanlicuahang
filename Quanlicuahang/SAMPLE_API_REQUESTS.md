# Sample API Calls and Test Data

This file contains practical examples of API calls with realistic data for testing the product management system.

## Sample Request Files for Testing

### 1. Create Complete T-Shirt Product with Color and Size Variants

```json
// POST /api/product_management/create_complete_product
{
  "product": {
    "code": "TSHIRT-COTTON-001",
    "name": "Premium Cotton T-Shirt",
    "barcode": "8901234567890",
    "price": 29.99,
    "unit": "pcs",
    "categoryId": "clothing-category-id",
    "supplierId": "textile-supplier-id",
    "variants": [
      {
        "code": "TSHIRT-001-BLK-S",
        "name": "Premium Cotton T-Shirt - Black - Small",
        "sku": "SKU-TSHIRT-BLK-S",
        "priceAdjustment": 0,
        "stockQuantity": 50,
        "attributeValueIds": ["color-black-id", "size-small-id"]
      },
      {
        "code": "TSHIRT-001-BLK-M",
        "name": "Premium Cotton T-Shirt - Black - Medium",
        "sku": "SKU-TSHIRT-BLK-M",
        "priceAdjustment": 0,
        "stockQuantity": 100,
        "attributeValueIds": ["color-black-id", "size-medium-id"]
      },
      {
        "code": "TSHIRT-001-BLK-L",
        "name": "Premium Cotton T-Shirt - Black - Large",
        "sku": "SKU-TSHIRT-BLK-L",
        "priceAdjustment": 2.0,
        "stockQuantity": 75,
        "attributeValueIds": ["color-black-id", "size-large-id"]
      },
      {
        "code": "TSHIRT-001-WHT-S",
        "name": "Premium Cotton T-Shirt - White - Small",
        "sku": "SKU-TSHIRT-WHT-S",
        "priceAdjustment": 0,
        "stockQuantity": 60,
        "attributeValueIds": ["color-white-id", "size-small-id"]
      },
      {
        "code": "TSHIRT-001-WHT-M",
        "name": "Premium Cotton T-Shirt - White - Medium",
        "sku": "SKU-TSHIRT-WHT-M",
        "priceAdjustment": 0,
        "stockQuantity": 120,
        "attributeValueIds": ["color-white-id", "size-medium-id"]
      },
      {
        "code": "TSHIRT-001-WHT-L",
        "name": "Premium Cotton T-Shirt - White - Large",
        "sku": "SKU-TSHIRT-WHT-L",
        "priceAdjustment": 2.0,
        "stockQuantity": 80,
        "attributeValueIds": ["color-white-id", "size-large-id"]
      }
    ]
  },
  "newAttributes": [
    {
      "code": "COLOR",
      "name": "Màu sắc",
      "description": "Các màu sắc có sẵn của sản phẩm",
      "values": [
        { "value": "Đen", "displayOrder": 1 },
        { "value": "Trắng", "displayOrder": 2 },
        { "value": "Xám", "displayOrder": 3 },
        { "value": "Xanh navy", "displayOrder": 4 },
        { "value": "Đỏ", "displayOrder": 5 }
      ]
    },
    {
      "code": "SIZE",
      "name": "Kích cỡ",
      "description": "Kích cỡ áo",
      "values": [
        { "value": "XS", "displayOrder": 1 },
        { "value": "S", "displayOrder": 2 },
        { "value": "M", "displayOrder": 3 },
        { "value": "L", "displayOrder": 4 },
        { "value": "XL", "displayOrder": 5 },
        { "value": "XXL", "displayOrder": 6 }
      ]
    }
  ]
}
```

### 2. Create Shoes with Size and Color Attributes

```json
// First create attributes
// POST /api/product_management/create_attribute_with_values
{
  "code": "SHOE_SIZE",
  "name": "Kích cỡ giày",
  "description": "Kích cỡ giày theo tiêu chuẩn",
  "values": [
    {"value": "35", "displayOrder": 1},
    {"value": "36", "displayOrder": 2},
    {"value": "37", "displayOrder": 3},
    {"value": "38", "displayOrder": 4},
    {"value": "39", "displayOrder": 5},
    {"value": "40", "displayOrder": 6},
    {"value": "41", "displayOrder": 7},
    {"value": "42", "displayOrder": 8},
    {"value": "43", "displayOrder": 9},
    {"value": "44", "displayOrder": 10},
    {"value": "45", "displayOrder": 11}
  ]
}

// POST /api/product/create_with_variants
{
  "code": "SNEAKER-AIR-001",
  "name": "Air Flow Sneakers",
  "barcode": "8901234567891",
  "price": 89.99,
  "unit": "đôi",
  "categoryId": "footwear-category-id",
  "supplierId": "shoe-supplier-id",
  "variants": [
    {
      "code": "SNEAKER-001-BLK-39",
      "name": "Air Flow Sneakers - Đen - Size 39",
      "sku": "SKU-SNEAKER-BLK-39",
      "priceAdjustment": 0,
      "stockQuantity": 25,
      "attributeValueIds": ["color-black-id", "shoe-size-39-id"]
    },
    {
      "code": "SNEAKER-001-BLK-40",
      "name": "Air Flow Sneakers - Đen - Size 40",
      "sku": "SKU-SNEAKER-BLK-40",
      "priceAdjustment": 0,
      "stockQuantity": 30,
      "attributeValueIds": ["color-black-id", "shoe-size-40-id"]
    },
    {
      "code": "SNEAKER-001-WHT-39",
      "name": "Air Flow Sneakers - Trắng - Size 39",
      "sku": "SKU-SNEAKER-WHT-39",
      "priceAdjustment": 5.00,
      "stockQuantity": 20,
      "attributeValueIds": ["color-white-id", "shoe-size-39-id"]
    }
  ]
}
```

### 3. Electronics with Technical Specifications

```json
// Create technical attributes
// POST /api/product_management/create_attribute_with_values
{
  "code": "STORAGE",
  "name": "Dung lượng lưu trữ",
  "description": "Dung lượng lưu trữ của thiết bị",
  "values": [
    {"value": "64GB", "displayOrder": 1},
    {"value": "128GB", "displayOrder": 2},
    {"value": "256GB", "displayOrder": 3},
    {"value": "512GB", "displayOrder": 4},
    {"value": "1TB", "displayOrder": 5}
  ]
}

// POST /api/product_management/create_attribute_with_values
{
  "code": "PHONE_COLOR",
  "name": "Màu điện thoại",
  "description": "Màu sắc điện thoại",
  "values": [
    {"value": "Space Gray", "displayOrder": 1},
    {"value": "Silver", "displayOrder": 2},
    {"value": "Gold", "displayOrder": 3},
    {"value": "Rose Gold", "displayOrder": 4},
    {"value": "Blue", "displayOrder": 5},
    {"value": "Green", "displayOrder": 6}
  ]
}

// Create phone product with variants
// POST /api/product/create_with_variants
{
  "code": "PHONE-PRO-2024",
  "name": "ProPhone 2024",
  "barcode": "8901234567892",
  "price": 999.99,
  "unit": "chiếc",
  "categoryId": "electronics-category-id",
  "supplierId": "tech-supplier-id",
  "variants": [
    {
      "code": "PHONE-PRO-2024-GRAY-128",
      "name": "ProPhone 2024 - Space Gray - 128GB",
      "sku": "SKU-PHONE-GRAY-128",
      "priceAdjustment": 0,
      "stockQuantity": 15,
      "attributeValueIds": ["phone-color-gray-id", "storage-128gb-id"]
    },
    {
      "code": "PHONE-PRO-2024-GRAY-256",
      "name": "ProPhone 2024 - Space Gray - 256GB",
      "sku": "SKU-PHONE-GRAY-256",
      "priceAdjustment": 200.00,
      "stockQuantity": 10,
      "attributeValueIds": ["phone-color-gray-id", "storage-256gb-id"]
    },
    {
      "code": "PHONE-PRO-2024-BLUE-128",
      "name": "ProPhone 2024 - Blue - 128GB",
      "sku": "SKU-PHONE-BLUE-128",
      "priceAdjustment": 0,
      "stockQuantity": 12,
      "attributeValueIds": ["phone-color-blue-id", "storage-128gb-id"]
    }
  ]
}
```

### 4. Search and Filter Examples

```json
// Search products by category
// POST /api/product/pagination
{
  "skip": 0,
  "take": 10,
  "where": {
    "categoryId": "clothing-category-id",
    "minPrice": 20.00,
    "maxPrice": 50.00
  }
}

// Search variants by product
// POST /api/product_variant/pagination
{
  "skip": 0,
  "take": 20,
  "where": {
    "productId": "product-id",
    "inStock": true
  }
}

// Search attribute values
// POST /api/product_attribute_value/pagination
{
  "skip": 0,
  "take": 10,
  "where": {
    "attributeId": "color-attribute-id",
    "isDeleted": false
  }
}
```

### 5. Getting Variant Suggestions

```json
// Get suggestions for creating a new variant
// POST /api/product_management/variant_suggestions
{
  "productId": "TSHIRT-COTTON-001-ID",
  "selectedAttributeValueIds": ["color-red-id", "size-xl-id"]
}

// Expected response:
{
  "suggestedName": "Đỏ - XL",
  "suggestedCode": "TSHIRT-COTTON-001-ĐỏXL",
  "selectedAttributes": [
    {
      "attributeId": "color-attr-id",
      "attributeName": "Màu sắc",
      "attributeCode": "COLOR",
      "selectedValue": "Đỏ",
      "selectedValueId": "color-red-id"
    },
    {
      "attributeId": "size-attr-id",
      "attributeName": "Kích cỡ",
      "attributeCode": "SIZE",
      "selectedValue": "XL",
      "selectedValueId": "size-xl-id"
    }
  ],
  "baseProduct": {
    "id": "TSHIRT-COTTON-001-ID",
    "code": "TSHIRT-COTTON-001",
    "name": "Premium Cotton T-Shirt",
    "price": 29.99
  }
}
```

### 6. Workflow Example: Complete Setup Process

1. **Create Categories and Suppliers first** (assuming these exist)

2. **Create Attributes:**

```bash
# Create Color attribute
curl -X POST "http://localhost:5000/api/product_management/create_attribute_with_values" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "COLOR",
    "name": "Màu sắc",
    "description": "Màu sắc sản phẩm",
    "values": [
      {"value": "Đen", "displayOrder": 1},
      {"value": "Trắng", "displayOrder": 2},
      {"value": "Xám", "displayOrder": 3}
    ]
  }'

# Create Size attribute
curl -X POST "http://localhost:5000/api/product_management/create_attribute_with_values" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "SIZE",
    "name": "Kích cỡ",
    "description": "Kích cỡ sản phẩm",
    "values": [
      {"value": "S", "displayOrder": 1},
      {"value": "M", "displayOrder": 2},
      {"value": "L", "displayOrder": 3}
    ]
  }'
```

3. **Get Attribute Values (for reference):**

```bash
curl -X GET "http://localhost:5000/api/product_management/attributes/select_box" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

4. **Create Complete Product:**

```bash
curl -X POST "http://localhost:5000/api/product_management/create_complete_product" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d @complete_product_request.json
```

This workflow ensures proper setup and testing of the entire product management system.
