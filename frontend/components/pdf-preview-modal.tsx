"use client";

import { useEffect, useRef } from "react";
import { X, Download, ExternalLink } from "lucide-react";
import { Button } from "@/components/ui/button";

interface PdfPreviewModalProps {
  url: string;
  filename?: string;
  onClose: () => void;
  labels: {
    download: string;
    openFullscreen: string;
  };
}

export function PdfPreviewModal({
  url,
  filename = "document.pdf",
  onClose,
  labels,
}: PdfPreviewModalProps) {
  const backdropRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handleKey);
    document.body.style.overflow = "hidden";
    return () => {
      document.removeEventListener("keydown", handleKey);
      document.body.style.overflow = "";
    };
  }, [onClose]);

  const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === backdropRef.current) onClose();
  };

  return (
    <div
      ref={backdropRef}
      onClick={handleBackdropClick}
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4"
    >
      <div className="bg-white rounded-xl shadow-2xl flex flex-col w-full max-w-4xl h-[90vh]">
        {/* Header */}
        <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200 shrink-0">
          <span className="text-sm font-medium text-gray-700 truncate max-w-xs">{filename}</span>
          <div className="flex items-center gap-2">
            <a href={url} download={filename}>
              <Button variant="outline" size="sm">
                <Download className="h-3.5 w-3.5 mr-1.5" />
                {labels.download}
              </Button>
            </a>
            <a href={url} target="_blank" rel="noopener noreferrer">
              <Button variant="outline" size="sm">
                <ExternalLink className="h-3.5 w-3.5 mr-1.5" />
                {labels.openFullscreen}
              </Button>
            </a>
            <Button variant="ghost" size="sm" onClick={onClose}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        </div>

        {/* PDF Viewer */}
        <iframe
          src={url}
          className="flex-1 w-full rounded-b-xl"
          title={filename}
        />
      </div>
    </div>
  );
}
