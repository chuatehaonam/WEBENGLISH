# 🎨 Monochromatic Blue Website Stylesheet

Hệ thống CSS hiện đại với **bảng màu xanh đơn sắc** chuyên nghiệp, được thiết kế để tạo ra những trang web đẹp mắt và nhất quán về mặt thị giác.

## 🌈 Color Palette

### Primary Blue Shades
- `--primary-darkest: #0a1929` - Navy Dark
- `--primary-darker: #1e3a8a` - Navy  
- `--primary-dark: #1d4ed8` - Blue Dark
- `--primary: #3b82f6` - Blue Primary
- `--primary-light: #60a5fa` - Blue Light
- `--primary-lighter: #93c5fd` - Blue Lighter
- `--primary-lightest: #dbeafe` - Blue Very Light

### Neutral Blues
- `--neutral-darkest: #1e293b` - Slate Dark
- `--neutral-dark: #334155` - Slate
- `--neutral: #64748b` - Slate Light
- `--neutral-light: #94a3b8` - Slate Lighter
- `--neutral-lighter: #cbd5e1` - Slate Very Light
- `--neutral-lightest: #f1f5f9` - Slate Minimal

### Accent Colors
- `--accent: #0ea5e9` - Sky Blue
- `--accent-light: #38bdf8` - Sky Blue Light

## 🚀 Features

### ✨ Tính năng chính
- **CSS Variables** cho easy customization
- **Responsive design** với mobile-first approach
- **Modern typography** với font Inter
- **Smooth animations** và transitions
- **Dark mode support** tự động
- **Component-based** architecture

### 🎯 Components bao gồm:
- **Navigation** - Navbar với sticky positioning
- **Buttons** - Primary, Secondary, Outline variants
- **Cards** - Với hover effects đẹp mắt
- **Forms** - Styled inputs với focus states
- **Alerts** - Info, Success, Warning, Error
- **Badges** - Primary, Secondary, Outline
- **Grid System** - 1-4 columns responsive
- **Typography** - Headings với gradient colors
- **Hero Section** - Với background gradient

## 📖 Usage

### 1. Basic Setup
```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Your Website</title>
    <link rel="stylesheet" href="styles.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
</head>
```

### 2. Layout Structure
```html
<div class="container">
    <section class="section">
        <div class="grid grid-cols-3">
            <!-- Your content here -->
        </div>
    </section>
</div>
```

### 3. Button Examples
```html
<button class="btn btn-primary">Primary Button</button>
<button class="btn btn-secondary">Secondary Button</button>
<button class="btn btn-outline">Outline Button</button>
<button class="btn btn-primary btn-lg">Large Button</button>
```

### 4. Card Component
```html
<div class="card">
    <div class="card-header">
        <h4>Card Title</h4>
    </div>
    <div class="card-body">
        <p>Card content goes here...</p>
    </div>
    <div class="card-footer">
        <button class="btn btn-primary">Action</button>
    </div>
</div>
```

### 5. Navigation
```html
<nav class="navbar">
    <div class="container">
        <a href="#" class="navbar-brand">Your Brand</a>
        <ul class="navbar-nav">
            <li><a href="#home">Home</a></li>
            <li><a href="#about">About</a></li>
            <li><a href="#contact">Contact</a></li>
        </ul>
    </div>
</nav>
```

### 6. Grid System
```html
<!-- 3 columns on desktop, 1 on mobile -->
<div class="grid grid-cols-3">
    <div>Column 1</div>
    <div>Column 2</div>
    <div>Column 3</div>
</div>

<!-- 2 columns -->
<div class="grid grid-cols-2">
    <div>Column 1</div>
    <div>Column 2</div>
</div>
```

### 7. Form Elements
```html
<form>
    <div class="form-group">
        <label for="email" class="form-label">Email</label>
        <input type="email" id="email" class="form-control" placeholder="your@email.com">
    </div>
    <button type="submit" class="btn btn-primary">Submit</button>
</form>
```

### 8. Alerts
```html
<div class="alert alert-info">This is an info alert!</div>
<div class="alert alert-success">Success message!</div>
<div class="alert alert-warning">Warning message!</div>
<div class="alert alert-error">Error message!</div>
```

### 9. Badges
```html
<span class="badge badge-primary">Primary</span>
<span class="badge badge-secondary">Secondary</span>
<span class="badge badge-outline">Outline</span>
```

## 🎨 Customization

### Thay đổi màu chính:
```css
:root {
    --primary: #your-color;
    --primary-dark: #your-darker-color;
    --primary-light: #your-lighter-color;
}
```

### Custom spacing:
```css
:root {
    --spacing-md: 1.5rem; /* Thay vì 1rem */
    --spacing-lg: 2rem;   /* Thay vì 1.5rem */
}
```

## 📱 Responsive Breakpoints

- **Desktop**: > 768px
- **Tablet**: ≤ 768px
- **Mobile**: ≤ 480px

## 🎭 Utility Classes

### Spacing
- Margin: `.mt-0` to `.mt-5`, `.mb-0` to `.mb-5`
- Padding: `.p-0` to `.p-5`

### Text
- Colors: `.text-primary`, `.text-secondary`, `.text-muted`, `.text-white`
- Alignment: `.text-center`, `.text-left`, `.text-right`

### Background
- `.bg-primary`, `.bg-secondary`, `.bg-light`

### Border & Shadow
- `.rounded`, `.rounded-lg`
- `.shadow`, `.shadow-lg`

### Flexbox
- `.flex`, `.flex-col`, `.flex-wrap`
- `.items-center`, `.justify-center`, `.justify-between`

## 🌙 Dark Mode

CSS framework tự động hỗ trợ dark mode dựa trên system preference:

```css
@media (prefers-color-scheme: dark) {
    /* Dark mode styles */
}
```

## 🎬 Animations

### Built-in animations:
- `.fade-in-up` - Fade in from bottom
- `.pulse` - Pulsing effect

### Custom animations:
```css
.my-element {
    transition: all var(--transition-normal);
}
```

## 📁 File Structure

```
project/
│
├── styles.css          # Main CSS file
├── index.html          # Demo HTML file
└── README.md          # Documentation
```

## 🏆 Best Practices

1. **Sử dụng CSS Variables** thay vì hard-coded colors
2. **Mobile-first approach** khi viết responsive code
3. **Semantic HTML** với proper heading hierarchy
4. **Accessibility** - đảm bảo contrast ratio tốt
5. **Performance** - tối ưu CSS selectors

## 🔧 Browser Support

- ✅ Chrome 88+
- ✅ Firefox 85+
- ✅ Safari 14+
- ✅ Edge 88+

## 📞 Support

Nếu bạn có bất kỳ câu hỏi nào về việc sử dụng CSS framework này, hãy tạo issue hoặc liên hệ trực tiếp.

---

**Tạo ra những website đẹp mắt với Monochromatic Blue CSS Framework! 🎨✨** 