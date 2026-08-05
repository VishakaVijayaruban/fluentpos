import { Upload } from "src/app/core/models/uploads/upload";

export interface Product {
  id: string;
  name: string;
  localeName: string;
  brandId: string;
  brandName: string;
  categoryId: string;
  categoryName: string;
  price: number;
  cost: number;
  imageUrl: string;
  // VAT percentage resolved server-side from the linked VAT rate (read-only).
  tax: number;
  vatRateId: string;
  barcode?: string;
  barcodeSymbology: string;
  detail: string;
  uploadRequest?: Upload;
}
