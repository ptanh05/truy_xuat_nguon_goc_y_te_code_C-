"use client"

import type React from "react"

import { useState, useRef, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Camera, Upload, X } from "lucide-react"
import { Html5Qrcode } from "html5-qrcode"

interface QRScannerProps {
  onScan: (result: string) => void
}

export default function QRScanner({ onScan }: QRScannerProps) {
  const [isScanning, setIsScanning] = useState(false)
  const [error, setError] = useState("")
  const [scanResult, setScanResult] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const scannerRef = useRef<Html5Qrcode | null>(null)
  const scannerElementId = "qr-reader"

  useEffect(() => {
    // Cleanup khi component unmount
    return () => {
      if (scannerRef.current && isScanning) {
        scannerRef.current
          .stop()
          .then(() => {
            scannerRef.current = null
          })
          .catch(() => {
            scannerRef.current = null
          })
      }
    }
  }, [isScanning])

  const startScanning = async () => {
    setIsScanning(true)
    setError("")
    setScanResult(null)

    try {
      const html5QrCode = new Html5Qrcode(scannerElementId)
      scannerRef.current = html5QrCode

      await html5QrCode.start(
        { facingMode: "environment" }, // Sử dụng camera sau
        {
          fps: 10,
          qrbox: { width: 250, height: 250 },
          aspectRatio: 1.0,
        },
        (decodedText) => {
          // QR code được quét thành công
          setScanResult(decodedText)
          onScan(decodedText)
          stopScanning()
        },
        (errorMessage) => {
          // Bỏ qua lỗi quét (chưa tìm thấy QR code)
          // Chỉ hiển thị lỗi nếu là lỗi nghiêm trọng
        }
      )
    } catch (err: any) {
      console.error("Error starting QR scanner:", err)
      setError(
        err.message?.includes("Permission")
          ? "Vui lòng cấp quyền truy cập camera"
          : "Không thể truy cập camera. Vui lòng thử lại."
      )
      setIsScanning(false)
      scannerRef.current = null
    }
  }

  const stopScanning = async () => {
    if (scannerRef.current) {
      try {
        await scannerRef.current.stop()
        await scannerRef.current.clear()
      } catch (err) {
        console.error("Error stopping scanner:", err)
      }
      scannerRef.current = null
    }
    setIsScanning(false)
  }

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    setError("")
    setScanResult(null)

    try {
      const html5QrCode = new Html5Qrcode(scannerElementId)
      
      const result = await html5QrCode.scanFile(file, false)
      setScanResult(result)
      onScan(result)
      
      // Reset file input
      if (fileInputRef.current) {
        fileInputRef.current.value = ""
      }
    } catch (err: any) {
      console.error("Error reading QR from image:", err)
      setError(
        err.message?.includes("No QR code")
          ? "Không tìm thấy QR code trong ảnh. Vui lòng chọn ảnh khác."
          : "Không thể đọc QR code từ ảnh. Vui lòng thử lại."
      )
    }
  }

  return (
    <div className="space-y-4">
      {/* Camera Scanner */}
      <div className="border-2 border-dashed border-gray-300 rounded-lg p-4 text-center">
        {isScanning ? (
          <div className="space-y-4">
            <div id={scannerElementId} className="w-full max-w-md mx-auto"></div>
            <div className="flex flex-col items-center gap-2">
              <p className="text-sm text-gray-600">Đang quét QR code...</p>
              <Button variant="outline" onClick={stopScanning} className="w-full max-w-xs">
                <X className="w-4 h-4 mr-2" />
                Dừng quét
              </Button>
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            <Camera className="w-16 h-16 text-gray-400 mx-auto" />
            <div>
              <h3 className="font-medium mb-2">Quét QR bằng camera</h3>
              <p className="text-sm text-gray-600 mb-4">
                Nhấn nút bên dưới để bật camera và quét QR code
              </p>
              <Button onClick={startScanning} disabled={isScanning}>
                <Camera className="w-4 h-4 mr-2" />
                Bật camera
              </Button>
            </div>
          </div>
        )}
      </div>

      {error && (
        <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-red-600 text-sm text-center">{error}</p>
        </div>
      )}

      {scanResult && (
        <div className="p-3 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-green-800 text-sm font-medium mb-1">Đã quét thành công:</p>
          <p className="text-green-700 text-xs font-mono break-all">{scanResult}</p>
        </div>
      )}

      {/* File Upload Alternative */}
      {!isScanning && (
        <div className="text-center">
          <p className="text-sm text-gray-600 mb-2">Hoặc</p>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            onChange={handleFileUpload}
            className="hidden"
          />
          <Button
            variant="outline"
            onClick={() => fileInputRef.current?.click()}
            disabled={isScanning}
          >
            <Upload className="w-4 h-4 mr-2" />
            Tải ảnh QR lên
          </Button>
        </div>
      )}

      <div className="text-xs text-gray-500 text-center">
        <p>💡 Mẹo: Đảm bảo QR code rõ nét và đủ ánh sáng</p>
      </div>
    </div>
  )
}
