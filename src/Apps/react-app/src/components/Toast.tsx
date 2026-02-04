import * as Toast from '@radix-ui/react-toast'
import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react'

type ToastType = 'success' | 'error' | 'info' | 'warning'

type ToastData = {
    id: string
    title: string
    description?: string
    type: ToastType
    duration: number
    open: boolean
}

type ToastContextType = {
    showToast: (title: string, description?: string, type?: ToastType, duration?: number) => void
}

const ToastContext = createContext<ToastContextType | null>(null)

export function useToast() {
    const context = useContext(ToastContext)
    if (!context) {
        throw new Error('useToast must be used within ToastProvider')
    }
    return context
}

type ToastProviderProps = {
    children: ReactNode
}

const DEFAULT_DURATION = 3000

function ToastItem({ toast, onRemove }: { toast: ToastData; onRemove: (id: string) => void }) {
    const [open, setOpen] = useState(true)

    useEffect(() => {
        const timer = setTimeout(() => {
            setOpen(false)
        }, toast.duration)
        return () => clearTimeout(timer)
    }, [toast.duration])

    useEffect(() => {
        if (!open) {
            // Delay removal to allow exit animation
            const timer = setTimeout(() => {
                onRemove(toast.id)
            }, 200)
            return () => clearTimeout(timer)
        }
    }, [open, toast.id, onRemove])

    return (
        <Toast.Root
            className={`toast-root toast-${toast.type}`}
            open={open}
            onOpenChange={setOpen}
        >
            <div className="toast-content">
                <div className="toast-icon">
                    {toast.type === 'success' && '✓'}
                    {toast.type === 'error' && '✕'}
                    {toast.type === 'warning' && '⚠'}
                    {toast.type === 'info' && 'ℹ'}
                </div>
                <div className="toast-text">
                    <Toast.Title className="toast-title">{toast.title}</Toast.Title>
                    {toast.description && (
                        <Toast.Description className="toast-description">
                            {toast.description}
                        </Toast.Description>
                    )}
                </div>
            </div>
            <Toast.Close className="toast-close" aria-label="Đóng">
                ✕
            </Toast.Close>
            <div
                className="toast-progress"
                style={{ animationDuration: `${toast.duration}ms` }}
            />
        </Toast.Root>
    )
}

export function ToastProvider({ children }: ToastProviderProps) {
    const [toasts, setToasts] = useState<ToastData[]>([])

    const showToast = useCallback((
        title: string,
        description?: string,
        type: ToastType = 'info',
        duration: number = DEFAULT_DURATION
    ) => {
        const id = `${Date.now()}-${Math.random().toString(36).slice(2)}`
        setToasts((prev) => [...prev, { id, title, description, type, duration, open: true }])
    }, [])

    const removeToast = useCallback((id: string) => {
        setToasts((prev) => prev.filter((t) => t.id !== id))
    }, [])

    return (
        <ToastContext.Provider value={{ showToast }}>
            <Toast.Provider swipeDirection="right">
                {children}
                {toasts.map((toast) => (
                    <ToastItem key={toast.id} toast={toast} onRemove={removeToast} />
                ))}
                <Toast.Viewport className="toast-viewport" />
            </Toast.Provider>
        </ToastContext.Provider>
    )
}

export default ToastProvider
