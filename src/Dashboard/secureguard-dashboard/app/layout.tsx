import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "SecureGuard DLP - Admin Dashboard",
  description: "Data Loss Prevention & Endpoint Monitoring System",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="font-sans antialiased">{children}</body>
    </html>
  );
}
