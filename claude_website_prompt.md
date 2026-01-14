# Claude Prompt - SimLock Website Development

Copy and paste everything below this line into a new Claude session:

---

## Project Context

I need you to build a professional website to sell SimLock software. First, read the project summary for full context:

**Read this file first:** `/home/csolaiman/SimLock/summary.md`

## Key Project Information

### Locations & Access
- **Project location:** `/home/csolaiman/SimLock`
- **Activation server:** `192.168.88.197` (SSH: `ssh -i ~/.ssh/dms-deploy csolaiman@192.168.88.197`)
- **Windows share:** `//192.168.88.156/Users/cstk421/Downloads/Dev` (mounted at `/mnt/windev` on activation server)
- **GitHub repo:** `https://github.com/cstk421/SimWoods-Golf`
- **GitHub PAT:** (stored in bash history on dev machine - run `history | grep github_pat` to retrieve)

### Activation Server Details
- **Server:** `192.168.88.197:8443`
- **Admin UI:** `https://activation.neutrocorp.com:8443/`
- **Admin credentials:** `admin` / `SimLock2024!`
- **Database:** SQLite (`/home/csolaiman/activation-server/activations.db`)
- **Flask app:** `/home/csolaiman/activation-server/app.py`

### API Endpoints (existing)
- `POST /api/check-email` - Check email for licenses
- `POST /api/activate` - Activate license on machine
- `POST /api/deactivate` - Deactivate license
- `POST /api/check` - Check activation status

---

## Website Requirements

### Overview
Build a clean, professional website to sell SimLock software. The website will be:
- **Domain:** `simlock.golf`
- **Hosted on:** Activation server (192.168.88.197) via Docker
- **Tech stack:** Your choice (suggest: Flask/FastAPI with Jinja2, or static HTML with nginx)

### Design Guidelines
- Clean, modern, professional design
- Golf-themed color scheme (greens, whites)
- Use screenshots from `/home/csolaiman/SimLock/snaps/` folder
- Mobile responsive
- Fast loading

### Screenshots Available
**Admin Panel:**
- `Capture.PNG` - Login screen
- `Capture.2.PNG` - General tab
- `Capture3.PNG` - Appearance tab
- `Capture4.PNG` - Buttons tab
- `Capture5.PNG` - Video tab
- `Capture6.PNG` - Theme tab
- `Capture7.PNG` - Screen Text tab

**Lock Screen:**
- `splashscreen.png` - Welcome splash screen
- `mainmenu.png` - Main menu with buttons
- `pinentry.png` - PIN entry keypad
- `videoplayer.png` - Tutorial video player

---

## Page Structure (3 Pages)

### Page 1: Landing Page (Home)
**URL:** `simlock.golf` or `simlock.golf/`

**Content:**
1. **Hero Section**
   - Large headline: "SimLock - Professional Kiosk Lock Screen for Golf Simulators"
   - Subheadline describing the value proposition
   - CTA button: "Get Started" or "Purchase Now"

2. **YouTube Demo Video Placeholder**
   - Embedded video player area (placeholder for now)
   - Caption: "Watch SimLock in Action"

3. **Features Section**
   - Use lock screen screenshots (splashscreen.png, mainmenu.png, pinentry.png, videoplayer.png)
   - Key features:
     - Automatic activation when golf software starts
     - Customizable branding (logos, colors, backgrounds)
     - PIN protection for returning golfers
     - Built-in tutorial video player
     - Custom action buttons (open PDFs, websites, programs)
     - Full theme customization
     - License management system

4. **Admin Panel Preview**
   - Screenshots of admin tabs
   - Brief description of configuration options

5. **Pricing Section**
   - Single license pricing (you decide price structure)
   - **Purchase Button** → Links to Zoho payment page
   - The Zoho payment integration should:
     - Collect customer email
     - Collect quantity of licenses
     - On successful payment, call activation server API to add licenses
     - Send confirmation email to customer

6. **Footer**
   - Links to Support, Contact
   - Copyright: NeutroCorp LLC
   - Social links if available

### Page 2: Support Page
**URL:** `simlock.golf/support`

**Content:**
1. **Header:** "SimLock Support & Documentation"

2. **Admin Guide** (embedded in page body)
   - Display the content from `snaps/SimLock_Admin_Guide.html`
   - Create hyperlinked table of contents for navigation:
     - Overview
     - Installation
     - Getting Started
     - Admin Panel Configuration
     - Lock Screen Features
     - Licensing & Activation
     - Troubleshooting
     - Support

3. **Download Section**
   - Link to download installer (SimLock_Installer.zip)
   - System requirements

4. **FAQ Section** (optional)

### Page 3: Contact Page
**URL:** `simlock.golf/contact`

**Content:**
1. **Header:** "Contact Us"

2. **Company Information**
   - Fetch info from: https://www.neutrocorp.com
   - Also check Google Business page for NeutroCorp
   - Include:
     - Company name: NeutroCorp LLC
     - Main phone number
     - Email: info@neutrocorp.com
     - Address (if available from Google Business)
     - Business hours (if available)

3. **Contact Form** (optional)
   - Name, Email, Subject, Message
   - Sends to info@neutrocorp.com

4. **Support Email**
   - support@neutrocorp.com for technical issues

5. **Map** (optional)
   - Embed Google Maps if address available

---

## Zoho Payment Integration

### Requirements
1. Create a Zoho payment page/button for purchasing licenses
2. Collect:
   - Customer email (required)
   - Quantity of licenses (required)
   - Payment information

3. **Webhook/Callback on successful payment:**
   - Call activation server to create license record
   - You'll need to add a new API endpoint to `/home/csolaiman/activation-server/app.py`:

   ```python
   @app.route('/api/create-license', methods=['POST'])
   def create_license():
       # Expects: email, quantity, payment_id (from Zoho)
       # Creates license record in database
       # Returns license key
   ```

4. **Email confirmation:**
   - Send customer an email with:
     - License key
     - Download link
     - Getting started instructions

### Security
- Webhook should verify it's from Zoho (signature validation)
- Don't expose this endpoint publicly without authentication

---

## Docker Deployment

### Requirements
1. Create `Dockerfile` for the website
2. Create `docker-compose.yml` that includes:
   - Website container
   - Nginx reverse proxy (if needed)
   - Integration with existing activation server

3. Deploy on 192.168.88.197 alongside the activation server
4. Configure for domain `simlock.golf`

### Suggested Structure
```
/home/csolaiman/simlock-website/
├── Dockerfile
├── docker-compose.yml
├── app/
│   ├── main.py (or app.py)
│   ├── templates/
│   │   ├── index.html
│   │   ├── support.html
│   │   └── contact.html
│   ├── static/
│   │   ├── css/
│   │   ├── js/
│   │   └── images/
│   └── requirements.txt
└── nginx/
    └── nginx.conf
```

---

## Tasks Summary

1. **Research Phase**
   - Read `/home/csolaiman/SimLock/summary.md`
   - Fetch NeutroCorp info from neutrocorp.com and Google Business
   - Review screenshots in `/home/csolaiman/SimLock/snaps/`

2. **Backend Development**
   - Create website application (Flask/FastAPI recommended)
   - Add `/api/create-license` endpoint to activation server
   - Set up Zoho payment webhook handler

3. **Frontend Development**
   - Design and build 3 pages
   - Integrate screenshots
   - Style with golf theme

4. **Docker Setup**
   - Create Dockerfile and docker-compose.yml
   - Configure nginx for simlock.golf domain
   - Deploy on 192.168.88.197

5. **Integration**
   - Connect Zoho payment to license creation
   - Set up email notifications
   - Test end-to-end purchase flow

---

## Notes

- The activation server is already running Flask on port 8443
- You can either extend the existing Flask app or create a separate container
- SSL certificates may be needed for simlock.golf
- Keep the design simple and focused on conversion (getting purchases)

Please start by reading the summary.md file and exploring the codebase, then propose your implementation plan before writing code.
