import { BrowserRouter, Routes, Route } from 'react-router'
import Header from './components/Header'
import Footer from './components/Footer'
import { ToastProvider } from './components/Toast'
import Products from './pages/Products'
import Home from './pages/Home'
import ProductDetails from './pages/ProductDetails'
import Order from './pages/Order'
import History from './pages/History'
import OrderDetails from './pages/OrderDetails'
import PersonalProfile from './pages/PersonalProfile'
import SearchResult from './pages/SearchResult'
import { AuthProvider } from './contexts/AuthContext'
import { CartProvider } from './contexts/CartContext'
import ProtectedRoute from './components/ProtectedRoute'

function App() {
  return (
    <BrowserRouter>
      <ToastProvider>
        <AuthProvider>
          <CartProvider>
            <div className="app">
              <div className="nebula" />
              <div className="nebula" />
              <div className="nebula" />
              <div className="page">
                <Header />
                <main>
                  <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/products" element={<Products />} />
                    <Route path="/products/:slug" element={<ProductDetails />} />
                    <Route path="/order" element={<Order />} />
                    <Route path="/history" element={<ProtectedRoute><History /></ProtectedRoute>} />
                    <Route path="/history/:id" element={<ProtectedRoute><OrderDetails /></ProtectedRoute>} />
                    <Route path="/profile" element={<ProtectedRoute><PersonalProfile /></ProtectedRoute>} />
                    <Route path="/search" element={<SearchResult />} />
                  </Routes>
                </main>
                <Footer />
              </div>
            </div>
          </CartProvider>
        </AuthProvider>
      </ToastProvider>
    </BrowserRouter>
  )
}

export default App
