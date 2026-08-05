export interface Product {
  id: string;
  name: string;
  localeName: string;
  brandName: string;
  categoryName: string;
  price: number;
  cost: number;
  imageUrl: string;
  // VAT percentage resolved server-side from the linked VAT rate.
  tax: number;
  vatRateId: string;
  barcode?: string;
  barcodeSymbology: string;
  detail: string;
}
