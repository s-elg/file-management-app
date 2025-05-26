import React, { useState, useEffect } from 'react';

const API_BASE_URL = 'http://localhost:5167/api';

const FileManagementApp = () => {
  const [user, setUser] = useState(null);
  const [isLogin, setIsLogin] = useState(true);
  const [files, setFiles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: ''
  });
  const [token, setToken] = useState(null);

  useEffect(() => {
    if (user && token) {
      fetchFiles();
    }
  }, [user, token]);

  const showMessage = (msg) => {
    setMessage(msg);
    setTimeout(() => setMessage(''), 3000);
  };

  const handleAuth = async () => {
    setLoading(true);
    try {
      const endpoint = isLogin ? '/auth/login' : '/auth/register';
      const payload = isLogin 
        ? { username: formData.username, password: formData.password }
        : formData;

      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });

      const data = await response.json();

      if (data.success) {
        if (isLogin) {
          setUser({ username: data.data.username, email: data.data.email });
          setToken(data.data.token);
          showMessage('Giriş başarılı!');
        } else {
          showMessage('Kayıt başarılı! Şimdi giriş yapın.');
          setIsLogin(true);
        }
        setFormData({ username: '', email: '', password: '' });
      } else {
        showMessage(data.message || 'Hata oluştu');
      }
    } catch (error) {
      showMessage('Bağlantı hatası: ' + error.message);
    }
    setLoading(false);
  };

  const handleLogout = () => {
    setUser(null);
    setToken(null);
    setFiles([]);
    showMessage('Çıkış yapıldı');
  };

  const fetchFiles = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/files`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      const data = await response.json();
      if (data.success) {
        setFiles(data.data || []);
      }
    } catch (error) {
      showMessage('Dosyalar yüklenemedi');
    }
  };

  const handleFileUpload = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      showMessage('Dosya 5MB\'dan büyük olamaz!');
      return;
    }

    setLoading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch(`${API_BASE_URL}/files/upload`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
        body: formData,
      });

      const data = await response.json();
      if (data.success) {
        showMessage('Dosya yüklendi!');
        fetchFiles();
      } else {
        showMessage(data.message || 'Yüklenemedi');
      }
    } catch (error) {
      showMessage('Yükleme hatası');
    }
    setLoading(false);
    e.target.value = '';
  };

  const deleteFile = async (fileId) => {
    if (!confirm('Silmek istediğinizden emin misiniz?')) return;

    try {
      const response = await fetch(`${API_BASE_URL}/files/${fileId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` },
      });

      const data = await response.json();
      if (data.success) {
        showMessage('Dosya silindi!');
        fetchFiles();
      }
    } catch (error) {
      showMessage('Silme hatası');
    }
  };

  const formatFileSize = (bytes) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return Math.round(bytes / 1024) + ' KB';
    return Math.round(bytes / (1024 * 1024)) + ' MB';
  };

  // Giriş sayfası
  if (!user) {
    return (
      <div style={{ 
        minHeight: '100vh', 
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '20px'
      }}>
        <div style={{
          background: 'white',
          padding: '40px',
          borderRadius: '10px',
          width: '100%',
          maxWidth: '400px',
          boxShadow: '0 10px 25px rgba(0,0,0,0.2)'
        }}>
          <h1 style={{ textAlign: 'center', marginBottom: '30px', color: '#333' }}>
            📁 Dosya Yönetimi
          </h1>
          
          {message && (
            <div style={{
              padding: '10px',
              marginBottom: '20px',
              background: '#d4edda',
              border: '1px solid #c3e6cb',
              borderRadius: '5px',
              color: '#155724'
            }}>
              {message}
            </div>
          )}

          <div style={{ marginBottom: '20px' }}>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>
              Kullanıcı Adı
            </label>
            <input
              type="text"
              value={formData.username}
              onChange={(e) => setFormData({...formData, username: e.target.value})}
              style={{
                width: '100%',
                padding: '10px',
                border: '1px solid #ddd',
                borderRadius: '5px',
                fontSize: '16px'
              }}
            />
          </div>

          {!isLogin && (
            <div style={{ marginBottom: '20px' }}>
              <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>
                E-posta
              </label>
              <input
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({...formData, email: e.target.value})}
                style={{
                  width: '100%',
                  padding: '10px',
                  border: '1px solid #ddd',
                  borderRadius: '5px',
                  fontSize: '16px'
                }}
              />
            </div>
          )}

          <div style={{ marginBottom: '20px' }}>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>
              Şifre
            </label>
            <input
              type="password"
              value={formData.password}
              onChange={(e) => setFormData({...formData, password: e.target.value})}
              style={{
                width: '100%',
                padding: '10px',
                border: '1px solid #ddd',
                borderRadius: '5px',
                fontSize: '16px'
              }}
            />
          </div>

          <button
            onClick={handleAuth}
            disabled={loading}
            style={{
              width: '100%',
              padding: '12px',
              background: loading ? '#ccc' : '#007bff',
              color: 'white',
              border: 'none',
              borderRadius: '5px',
              fontSize: '16px',
              cursor: loading ? 'not-allowed' : 'pointer',
              marginBottom: '15px'
            }}
          >
            {loading ? 'İşleniyor...' : (isLogin ? 'Giriş Yap' : 'Kayıt Ol')}
          </button>

          <div style={{ textAlign: 'center' }}>
            <button
              onClick={() => setIsLogin(!isLogin)}
              style={{
                background: 'none',
                border: 'none',
                color: '#007bff',
                cursor: 'pointer',
                textDecoration: 'underline'
              }}
            >
              {isLogin ? 'Kayıt ol' : 'Giriş yap'}
            </button>
          </div>
        </div>
      </div>
    );
  }

  // Ana sayfa
  return (
    <div style={{ minHeight: '100vh', background: '#f5f5f5' }}>
      {/* Header */}
      <div style={{
        background: 'white',
        padding: '15px 20px',
        borderBottom: '1px solid #ddd',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}>
        <h1 style={{ margin: 0, color: '#333' }}>📁 Dosya Yönetimi</h1>
        <div>
          <span style={{ marginRight: '15px' }}>👤 {user.username}</span>
          <button
            onClick={handleLogout}
            style={{
              padding: '8px 15px',
              background: '#dc3545',
              color: 'white',
              border: 'none',
              borderRadius: '5px',
              cursor: 'pointer'
            }}
          >
            Çıkış
          </button>
        </div>
      </div>

      <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
        {message && (
          <div style={{
            padding: '10px',
            marginBottom: '20px',
            background: '#d4edda',
            border: '1px solid #c3e6cb',
            borderRadius: '5px',
            color: '#155724'
          }}>
            {message}
          </div>
        )}

        {/* Dosya Yükleme */}
        <div style={{
          background: 'white',
          padding: '30px',
          borderRadius: '10px',
          marginBottom: '20px',
          border: '2px dashed #007bff',
          textAlign: 'center'
        }}>
          <h2>📤 Dosya Yükle</h2>
          <p>PDF, PNG veya JPG dosyası seçin (Max: 5MB)</p>
          <input
            type="file"
            accept=".pdf,.png,.jpg,.jpeg"
            onChange={handleFileUpload}
            disabled={loading}
            style={{
              margin: '10px 0',
              padding: '10px',
              border: '1px solid #ddd',
              borderRadius: '5px'
            }}
          />
        </div>

        {/* Dosya Listesi */}
        <div style={{
          background: 'white',
          borderRadius: '10px',
          overflow: 'hidden'
        }}>
          <div style={{
            padding: '20px',
            borderBottom: '1px solid #ddd',
            background: '#f8f9fa'
          }}>
            <h2 style={{ margin: 0 }}>📋 Dosyalarım ({files.length})</h2>
          </div>

          {files.length === 0 ? (
            <div style={{ padding: '40px', textAlign: 'center', color: '#666' }}>
              Henüz dosya yüklenmemiş
            </div>
          ) : (
            files.map((file) => (
              <div key={file.id} style={{
                padding: '15px 20px',
                borderBottom: '1px solid #eee',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center'
              }}>
                <div>
                  <div style={{ fontWeight: 'bold', marginBottom: '5px' }}>
                    {file.contentType.includes('pdf') ? '📄' : '🖼️'} {file.originalFileName}
                  </div>
                  <div style={{ color: '#666', fontSize: '14px' }}>
                    {formatFileSize(file.fileSize)} • {new Date(file.uploadedAt).toLocaleDateString('tr-TR')}
                  </div>
                </div>
                <button
                  onClick={() => deleteFile(file.id)}
                  style={{
                    padding: '5px 10px',
                    background: '#dc3545',
                    color: 'white',
                    border: 'none',
                    borderRadius: '3px',
                    cursor: 'pointer'
                  }}
                >
                  🗑️ Sil
                </button>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};

export default FileManagementApp;