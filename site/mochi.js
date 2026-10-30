// =============================================================================
// REWIND PRESENTATION DECK — MOCHI CHARACTER & SLIDE CONTROLLER
// Handles:
// 1. Fullscreen presentation snap transitions (1 scroll = 1 slide)
// 2. Continuous traveling Mochi with position matching across stages
// 3. Dynamic expressions & animations (Cool Sunglasses -> Surprised -> Detective -> Proud -> Coin)
// 4. Dark & Light Theme State Machine
// =============================================================================

const MOCHI_CORE_SVG = `
<svg viewBox="0 0 100 120" class="mochi-actor-svg" id="mochiHeroSvg" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <radialGradient id="mochiPitGrad" cx="40%" cy="35%" r="65%">
      <stop offset="0%" stop-color="#DE7B2C"/>
      <stop offset="50%" stop-color="#B35B1C"/>
      <stop offset="90%" stop-color="#783408"/>
      <stop offset="100%" stop-color="#4A1E03"/>
    </radialGradient>
    <radialGradient id="mochiCoinGrad" cx="35%" cy="35%" r="65%">
      <stop offset="0%" stop-color="#FFFBEB"/>
      <stop offset="25%" stop-color="#FDE047"/>
      <stop offset="65%" stop-color="#CA8A04"/>
      <stop offset="100%" stop-color="#713F12"/>
    </radialGradient>
    <!-- ClipPath to frame ONLY Mochi's head/face on close-up with rounded organic chin -->
    <clipPath id="mochiHeadOnlyClip">
      <path d="M 10 0 L 90 0 L 90 55 C 90 73 70 74 50 74 C 30 74 10 73 10 55 Z" />
    </clipPath>
  </defs>

  <!-- Ground Drop Shadow -->
  <ellipse cx="50" cy="113" rx="30" ry="5.5" fill="rgba(0,0,0,0.38)" id="mochiShadow"/>

  <!-- Golden Commemorative Coin Backplate (Revealed on final footer settle) -->
  <g id="mochiCoinPlate" style="opacity:0; transform-origin:center; transform:scale(0.8);">
    <circle cx="50" cy="60" r="48" fill="url(#mochiCoinGrad)" stroke="#FEF08A" stroke-width="2.5" />
    <circle cx="50" cy="60" r="42" fill="none" stroke="#713F12" stroke-width="1.8" stroke-dasharray="3 4" />
    <text x="50" y="24" text-anchor="middle" font-family="monospace" font-size="5" fill="#713F12" font-weight="bold" letter-spacing="1.2">★ REWIND VERIFIED ★</text>
    <text x="50" y="103" text-anchor="middle" font-family="monospace" font-size="4.5" fill="#713F12" font-weight="bold" letter-spacing="1">LOCAL • ZERO DATA LOSS</text>
  </g>

  <!-- Character Group: Body, Limbs, Face -->
  <g id="mochiBodyGroup">
    <!-- Top Stem & Leaf Sprout with natural swaying physics -->
    <g id="mochiSprout" style="transform-origin: 50px 22px;">
      <path d="M 48 22 C 48 13 52 9 52 5 C 55 5 55 8 53 13 C 51 18 51 22 51 22 Z" fill="#633917" stroke="#381E09" stroke-width="2" stroke-linejoin="round"/>
      <path d="M 52 14 C 68 9 74 18 64 24 C 57 28 52 20 52 14 Z" fill="#4CAF50" stroke="#163814" stroke-width="2" stroke-linejoin="round"/>
    </g>

    <!-- Outer Avocado Rind (Pear Silhouette) -->
    <path d="M 50 18 C 68 18 78 32 78 54 C 84 72 88 96 76 104 C 64 112 36 112 24 104 C 12 96 16 72 22 54 C 22 32 32 18 50 18 Z"
          fill="#235020" stroke="#143212" stroke-width="3.5" stroke-linejoin="round"/>

    <!-- Inner Lime Margin -->
    <path d="M 50 21 C 66 21 75 34 75 54 C 81 71 85 93 74 101 C 63 109 37 109 26 101 C 15 93 19 71 25 54 C 25 34 34 21 50 21 Z"
          fill="#80C838"/>

    <!-- Honeydew Flesh -->
    <path d="M 50 24 C 64 24 71 36 71 55 C 77 71 80 91 71 98 C 61 105 39 105 29 98 C 20 91 23 71 29 55 C 29 36 36 24 50 24 Z"
          fill="#E6F587"/>

    <!-- Lower Body / Belly / Feet Group (Seamlessly faded in close-up so only head/face appears) -->
    <g id="mochiLowerBodyClipGroup">
      <!-- Feet (Dark Forest Avocado Green) -->
      <g id="mochiFootL" style="transform-origin: 39px 102px;">
        <path d="M 33 102 C 33 113 45 113 45 102 Z" fill="#2D5A27" stroke="#163814" stroke-width="2.5" stroke-linejoin="round"/>
      </g>
      <g id="mochiFootR" style="transform-origin: 61px 102px;">
        <path d="M 55 102 C 55 113 67 113 67 102 Z" fill="#2D5A27" stroke="#163814" stroke-width="2.5" stroke-linejoin="round"/>
      </g>

      <!-- Arms with kinetic waving/pointing transforms -->
      <g id="mochiArmL" style="transform-origin: 27px 58px;">
        <path d="M 27 55 C 16 56 16 68 25 69 C 27 69 29 67 30 64 Z" fill="#70B843" stroke="#163814" stroke-width="2.5" stroke-linejoin="round"/>
      </g>
      <g id="mochiArmR" style="transform-origin: 73px 58px;">
        <path d="M 73 55 C 84 56 84 68 75 69 C 73 69 71 67 70 64 Z" fill="#70B843" stroke="#163814" stroke-width="2.5" stroke-linejoin="round"/>
      </g>

      <!-- Belly Pit Seed -->
      <circle cx="50" cy="78" r="16" fill="#B35B1C" stroke="#66310C" stroke-width="2.5"/>
      <circle cx="50" cy="78" r="15" fill="url(#mochiPitGrad)"/>
      <path d="M 44 73 A 8 8 0 0 1 54 69" stroke="#FFFFFF" stroke-width="2" stroke-linecap="round" fill="none" opacity="0.7"/>
      <ellipse cx="45" cy="71" rx="2" ry="1.5" fill="#FFFFFF" opacity="0.85"/>
    </g>

    <!-- Cheeks Blush -->
    <ellipse cx="35.5" cy="49.5" rx="4.5" ry="3" fill="#FFA1A1" opacity="0.85" id="mochiBlushL"/>
    <ellipse cx="64.5" cy="49.5" rx="4.5" ry="3" fill="#FFA1A1" opacity="0.85" id="mochiBlushR"/>

    <!-- EXPRESSIONS CONTAINER -->
    <g id="mochiFaceGroup">
      <!-- 1. Normal Happy Eyes with Cool Sunglasses (Hammy Benchmark Style) -->
      <g id="exp_happy">
        <!-- Cool Tinted Sunglasses Group with Specular Reflection -->
        <g id="mochiSunglasses">
          <!-- Glass frames -->
          <path d="M 32 40 Q 43 38 48 42 Q 50 42 52 42 Q 57 38 68 40 C 72 41 73 53 67 55 Q 56 56 51 51 C 49 49 48 48 45 51 Q 40 56 29 54 C 23 52 25 41 32 40 Z"
                fill="#111827" stroke="#030712" stroke-width="2.2" stroke-linejoin="round"/>
          <!-- Lens Specular Glint Reflection Lines -->
          <path d="M 33 43 L 44 52" stroke="rgba(255,255,255,0.7)" stroke-width="1.8" stroke-linecap="round"/>
          <path d="M 40 43 L 46 48" stroke="rgba(255,255,255,0.4)" stroke-width="1.2" stroke-linecap="round"/>
          <path d="M 55 43 L 64 52" stroke="rgba(255,255,255,0.7)" stroke-width="1.8" stroke-linecap="round"/>
          <path d="M 60 43 L 66 48" stroke="rgba(255,255,255,0.4)" stroke-width="1.2" stroke-linecap="round"/>
        </g>
        <!-- Confident Smirk Mouth -->
        <path d="M 46 59 Q 50 63 55 58" stroke="#141414" stroke-width="2.4" stroke-linecap="round" fill="none"/>
      </g>

      <!-- 2. Alert / Bug Found (Shocked Wide Pupils, Sunglasses Slide Down Nose, Mouth Behind Glasses) -->
      <g id="exp_alert" style="display:none; opacity:0;">
        <!-- Wide Cartoon Anime Eyes Visible -->
        <ellipse cx="42.5" cy="40" rx="5.8" ry="6.6" fill="#141414"/>
        <ellipse cx="44.8" cy="37.2" rx="2.6" ry="2.6" fill="#FFFFFF"/>
        <ellipse cx="57.5" cy="40" rx="5.8" ry="6.6" fill="#141414"/>
        <ellipse cx="59.8" cy="37.2" rx="2.6" ry="2.6" fill="#FFFFFF"/>

        <!-- Shocked Cartoon Sweat Drop -->
        <path d="M 70 33 C 72 30 74 34 72 37 C 70 38 68 37 68 35 C 68 33 70 33 70 33 Z" fill="#60A5FA" stroke="#1D4ED8" stroke-width="0.8"/>

        <!-- Open Gasp Mouth (Rendered BEHIND sunglasses) -->
        <ellipse cx="50" cy="58" rx="3.2" ry="4.2" fill="#141414"/>
        <ellipse cx="50" cy="60" rx="2" ry="1.6" fill="#F87171"/>

        <!-- Slid Down Sunglasses Resting Over Nose and Mouth -->
        <g id="mochiSunglassesSlid" style="transform: translateY(14px) rotate(4deg); transform-origin: 50px 45px;">
          <path d="M 32 40 Q 43 38 48 42 Q 50 42 52 42 Q 57 38 68 40 C 72 41 73 53 67 55 Q 56 56 51 51 C 49 49 48 48 45 51 Q 40 56 29 54 C 23 52 25 41 32 40 Z"
                fill="#111827" stroke="#030712" stroke-width="1.8" stroke-linejoin="round"/>
          <path d="M 33 43 L 44 52" stroke="rgba(255,255,255,0.6)" stroke-width="1.4" stroke-linecap="round"/>
          <path d="M 55 43 L 64 52" stroke="rgba(255,255,255,0.6)" stroke-width="1.4" stroke-linecap="round"/>
        </g>
      </g>

      <!-- 3. Focused Detective (Squinting Eyebrows, Concentrated Look) -->
      <g id="exp_focused" style="display:none; opacity:0;">
        <path d="M 36 38 L 47 42" stroke="#141414" stroke-width="2.8" stroke-linecap="round"/>
        <path d="M 64 38 L 53 42" stroke="#141414" stroke-width="2.8" stroke-linecap="round"/>
        <ellipse cx="42.5" cy="44" rx="4" ry="3.2" fill="#141414"/>
        <ellipse cx="43.8" cy="43" rx="1.6" ry="1.6" fill="#FFFFFF"/>
        <ellipse cx="57.5" cy="44" rx="4" ry="3.2" fill="#141414"/>
        <ellipse cx="58.8" cy="43" rx="1.6" ry="1.6" fill="#FFFFFF"/>
        <path d="M 47 50 L 53 49" stroke="#141414" stroke-width="2.4" stroke-linecap="round"/>
      </g>

      <!-- 4. Proud / Celebrating (Big Sparkling Twinkle Eyes & Sweet Curved Smile, Soft Blushing Cheeks) -->
      <g id="exp_proud" style="display:none; opacity:0;">
        <!-- Friendly round open eyes with twinkles -->
        <ellipse cx="42.5" cy="41" rx="5.2" ry="5.8" fill="#141414"/>
        <ellipse cx="44.2" cy="38.8" rx="2.2" ry="2.2" fill="#FFFFFF"/>
        <ellipse cx="41" cy="43.5" rx="1.0" ry="1.0" fill="#FFFFFF"/>
        
        <ellipse cx="57.5" cy="41" rx="5.2" ry="5.8" fill="#141414"/>
        <ellipse cx="59.2" cy="38.8" rx="2.2" ry="2.2" fill="#FFFFFF"/>
        <ellipse cx="56" cy="43.5" rx="1.0" ry="1.0" fill="#FFFFFF"/>
        
        <!-- Soft rosy blush cheeks -->
        <ellipse cx="34" cy="46" rx="4" ry="2.4" fill="#FCA5A5" opacity="0.85"/>
        <ellipse cx="66" cy="46" rx="4" ry="2.4" fill="#FCA5A5" opacity="0.85"/>
        
        <!-- Sweet gentle upward curve smile (Clean, cute, no tongue) -->
        <path d="M 44.5 50 Q 50 56 55.5 50" stroke="#141414" stroke-width="2.4" stroke-linecap="round" fill="none"/>
      </g>

      <!-- 5. Playful Wink (Left eye open with sparkle glint, right eye cheeky wink arc, rosy blush) -->
      <g id="exp_wink" style="display:none; opacity:0;">
        <!-- Right eye winking: playful arc ^ -->
        <path d="M 53 43 Q 58.5 35 64 43" stroke="#141414" stroke-width="3" stroke-linecap="round" fill="none"/>
        
        <!-- Left eye open, big round sparkle pupil -->
        <ellipse cx="42.5" cy="41" rx="5.2" ry="5.8" fill="#141414"/>
        <ellipse cx="44.2" cy="38.8" rx="2.2" ry="2.2" fill="#FFFFFF"/>
        <ellipse cx="41" cy="43.5" rx="1.0" ry="1.0" fill="#FFFFFF"/>
        
        <!-- Cheeks with cute blush -->
        <ellipse cx="33" cy="45" rx="4.5" ry="2.5" fill="#FCA5A5" opacity="0.9"/>
        <ellipse cx="67" cy="45" rx="4.5" ry="2.5" fill="#FCA5A5" opacity="0.9"/>
        
        <!-- Cute confident smirk smile -->
        <path d="M 45 49 Q 50 55 57 48" stroke="#141414" stroke-width="2.4" stroke-linecap="round" fill="none"/>
      </g>
    </g>
  </g>
</svg>
`;

class DeckMochiNavigator {
  constructor() {
    this.currentExpression = 'happy';
    this.isCoin = false;
    this.initStage();
    this.initOrganicBreathing();
    this.initDeckScrollTrigger();
  }

  initStage() {
    const existing = document.getElementById('mochiTravelerStage');
    if (existing) existing.remove();

    const stage = document.createElement('div');
    stage.className = 'mochi-traveler-stage';
    stage.id = 'mochiTravelerStage';
    stage.innerHTML = `
      <div class="mochi-actor" id="mochiActor" role="button" aria-label="Mochi QA Companion" tabindex="0">
        ${MOCHI_CORE_SVG}
      </div>
    `;
    document.body.appendChild(stage);

    const actor = document.getElementById('mochiActor');
    if (actor) {
      actor.addEventListener('click', () => this.cheer());
    }
  }

  initOrganicBreathing() {
    if (typeof gsap === 'undefined') return;

    const bodyGroup = document.getElementById('mochiBodyGroup');
    const sprout = document.getElementById('mochiSprout');
    const shadow = document.getElementById('mochiShadow');

    if (bodyGroup) {
      gsap.to(bodyGroup, {
        y: -6,
        scaleY: 1.025,
        scaleX: 0.985,
        duration: 2.2,
        ease: 'sine.inOut',
        yoyo: true,
        repeat: -1
      });
    }

    if (shadow) {
      gsap.to(shadow, {
        scaleX: 0.90,
        opacity: 0.25,
        duration: 2.2,
        ease: 'sine.inOut',
        yoyo: true,
        repeat: -1,
        transformOrigin: '50px 113px'
      });
    }

    if (sprout) {
      gsap.to(sprout, {
        rotation: 6,
        duration: 2.8,
        ease: 'sine.inOut',
        yoyo: true,
        repeat: -1,
        transformOrigin: '50px 22px'
      });
    }
  }

  setExpression(expression) {
    if (this.currentExpression === expression) return;
    this.currentExpression = expression;

    const expressions = ['happy', 'alert', 'focused', 'proud', 'wink'];
    expressions.forEach(exp => {
      const el = document.getElementById(`exp_${exp}`);
      if (el) {
        if (exp === expression) {
          el.style.display = 'block';
          if (typeof gsap !== 'undefined') {
            gsap.to(el, { opacity: 1, duration: 0.25, overwrite: 'auto' });
            if (exp === 'alert') {
              const glasses = document.getElementById('mochiSunglassesSlid');
              if (glasses) {
                gsap.fromTo(glasses, 
                  { y: -8, rotation: 0 }, 
                  { y: 14, rotation: 4, duration: 0.35, ease: 'bounce.out', overwrite: 'auto' }
                );
              }
            }
          } else {
            el.style.opacity = '1';
          }
        } else {
          el.style.display = 'none';
          el.style.opacity = '0';
        }
      }
    });
  }

  cheer() {
    if (typeof gsap === 'undefined') return;
    const actor = document.getElementById('mochiActor');
    const armL = document.getElementById('mochiArmL');
    const armR = document.getElementById('mochiArmR');
    const sprout = document.getElementById('mochiSprout');

    this.setExpression('proud');

    gsap.timeline()
      .to(actor, { y: '-=35', duration: 0.22, ease: 'power2.out' })
      .to(actor, { y: '+=35', duration: 0.28, ease: 'bounce.out' });

    if (armL && armR) {
      gsap.timeline()
        .to([armL, armR], { rotation: 30, duration: 0.15, yoyo: true, repeat: 3 });
    }
    if (sprout) {
      gsap.timeline()
        .to(sprout, { rotation: 22, duration: 0.1, yoyo: true, repeat: 3 });
    }
  }

  // Master Fullscreen Slide Story Progression
  initDeckScrollTrigger() {
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;

    const actor = document.getElementById('mochiActor');
    if (!actor) return;

    // Unscaled base element dimensions (CSS layout dimensions without transform)
    const getActorDims = () => {
      const w = actor.offsetWidth > 0 ? actor.offsetWidth : (window.innerWidth <= 420 ? 180 : (window.innerWidth <= 768 ? 210 : (window.innerWidth <= 1024 ? 260 : 320)));
      const h = actor.offsetHeight > 0 ? actor.offsetHeight : (w * 1.2);
      return { w, h, halfW: w / 2, halfH: h / 2 };
    };

    // Helper function to get center coordinates of an anchor element relative to viewport
    const getAnchorPos = (anchorId) => {
      const el = document.getElementById(anchorId);
      if (!el) return null;
      const rect = el.getBoundingClientRect();
      const dims = getActorDims();
      return {
        x: rect.left + (rect.width / 2) - dims.halfW,
        y: rect.top + (rect.height / 2) - dims.halfH
      };
    };

    // Hero 'O' Spotlight coordinates (defaults to center of viewport)
    const getHeroCirclePos = () => {
      const dims = getActorDims();
      const circle = document.getElementById('heroSpotlightCircle');
      if (circle) {
        const rect = circle.getBoundingClientRect();
        // If the element is within viewport and has valid dimensions
        if (rect.width > 0 && rect.height > 0) {
          return {
            x: rect.left + (rect.width / 2) - dims.halfW,
            y: rect.top + (rect.height / 2) - dims.halfH
          };
        }
      }
      return {
        x: (window.innerWidth / 2) - dims.halfW,
        y: (window.innerHeight / 2) - dims.halfH
      };
    };

    const heroCenter = getHeroCirclePos();

    gsap.set(actor, {
      x: heroCenter.x,
      y: heroCenter.y,
      scale: 1,
      rotation: 0
    });

    // Re-align after web fonts finish loading (as custom font metrics shift hero wordmark elements)
    if (document.fonts && document.fonts.ready) {
      document.fonts.ready.then(() => {
        if (window.scrollY === 0) {
          const freshCenter = getHeroCirclePos();
          gsap.set(actor, {
            x: freshCenter.x,
            y: freshCenter.y
          });
          ScrollTrigger.refresh();
        }
      });
    }

    // -------------------------------------------------------------
    // SLIDE 1: HERO -> CLOSE-UP BEAT (0.00 - 0.22)
    // - Sub-phase A (0.00 - 0.04): Initial Page Load state (M-[O]-CHI, Mochi centered in circle O)
    // - Sub-phase B (0.04 - 0.16): First scroll close up!
    //   * Mochi alone zooms in dramatically (scale ~ 3.2x) focusing on head/face
    //   * Sunglasses slide down nose, wide alert cartoon eyes emerge
    //   * Background wordmark ('M', 'CHI', 'O' circle, tagline, bottom cues) fade to 0 opacity
    //   * Lower body and limbs fade/translate down, framing ONLY the expressive head/face
    // - Sub-phase C (0.16 - 0.24): Smooth transition gliding out of close-up to Slide 2 Left Dock
    // -------------------------------------------------------------
    const heroWordmark = document.getElementById('heroWordmarkWrap');
    const heroTagline = document.querySelector('.hero-tagline-mount');
    const heroBottomBar = document.querySelector('.hero-bottom-bar-leveled');
    const heroGlow = document.getElementById('heroAmbientGlow');
    const mochiLowerBody = document.getElementById('mochiLowerBodyClipGroup');
    const mochiSideTextWrap = document.getElementById('mochiSideTextWrap');

    // Explicit discrete keyframe positions for each step (exact 1/5th fractions = each 100vh slide)
    const stepPositions = [0.00, 0.20, 0.40, 0.60, 0.80, 1.00];

    // SCROLL-DRIVEN TIMELINE WITH AUTOMATIC STEP-LOCK SNAP
    ScrollTrigger.create({
      trigger: '.slides-deck-wrapper',
      start: 'top top',
      end: 'bottom bottom',
      scrub: 0.3,
      snap: {
        snapTo: stepPositions,
        duration: { min: 0.35, max: 0.65 },
        delay: 0.05,
        ease: 'power2.out'
      },
      onUpdate: (self) => {
        const p = self.progress;
        const isMobile = window.innerWidth <= 1024;
        const currentDims = getActorDims();

        // Slide Indicators Update (6 dots)
        const dots = document.querySelectorAll('.story-slide-dot');
        let currentStep = 0;
        if (p < 0.10) currentStep = 0;
        else if (p < 0.30) currentStep = 1;
        else if (p < 0.50) currentStep = 2;
        else if (p < 0.70) currentStep = 3;
        else if (p < 0.90) currentStep = 4;
        else currentStep = 5;

        dots.forEach((dot, idx) => {
          if (idx === currentStep) dot.classList.add('active');
          else dot.classList.remove('active');
        });

        // -------------------------------------------------------------
        // STEP 0 -> STEP 1: HERO RESTING (0.00) -> CLOSE-UP BEAT (0.20)
        // -------------------------------------------------------------
        if (p < 0.30) {
          this.setCoinState(false);

          if (p <= 0.04) {
            // STEP 0: Resting Hero State on Page Load (M - [O] - CHI)
            // Align to heroSpotlightCircle ('O' letter), exactly as initialized on page load
            const heroTarget = getHeroCirclePos();
            gsap.to(actor, {
              x: heroTarget.x,
              y: heroTarget.y,
              scale: 1,
              transformOrigin: '50% 50%',
              rotation: 0,
              duration: 0.2,
              overwrite: 'auto'
            });

            // Restore full avocado body (remove head-only clip)
            const bodyGroup = document.getElementById('mochiBodyGroup');
            if (bodyGroup) bodyGroup.removeAttribute('clip-path');

            if (mochiLowerBody) {
              gsap.to(mochiLowerBody, { opacity: 1, y: 0, duration: 0.2, overwrite: 'auto' });
            }

            // Restore all hero background elements
            if (heroWordmark) gsap.to(heroWordmark, { opacity: 1, scale: 1, duration: 0.2, overwrite: 'auto' });
            if (heroTagline) gsap.to(heroTagline, { opacity: 1, duration: 0.2, overwrite: 'auto' });
            if (heroBottomBar) gsap.to(heroBottomBar, { opacity: 1, duration: 0.2, overwrite: 'auto' });
            if (heroGlow) gsap.to(heroGlow, { opacity: 0.95, duration: 0.2, overwrite: 'auto' });
            if (mochiSideTextWrap) gsap.to(mochiSideTextWrap, { opacity: 0, y: 15, duration: 0.2, overwrite: 'auto' });

            this.setExpression('happy');
          }
          else {
            // STEP 1: FIRST SCROLL SCOPE FOR MOCHI ALONE!
            // Cinematic Zoom-In: whole avocado body is full and intact (no clipping cutoffs),
            // centered in view with the lower body naturally bleeding off the bottom of the screen.
            const bodyGroup = document.getElementById('mochiBodyGroup');
            if (bodyGroup) {
              bodyGroup.removeAttribute('clip-path');
              bodyGroup.style.clipPath = 'none';
            }

            // High zoom scale for fullness across viewports
            const targetScale = window.innerWidth <= 480 ? 3.8 : (window.innerWidth <= 1024 ? 2.6 : 2.7);
            
            // Optical center calculations:
            // Keep top stem safely below navbar (~95px from top), head & eyes centered,
            // while full lower body/belly naturally extends below the viewport bottom
            const fullCenterX = (window.innerWidth / 2) - currentDims.halfW;
            const fullCenterY = 95 + (currentDims.h * (targetScale - 1) / 2);

            gsap.to(actor, {
              x: fullCenterX,
              y: fullCenterY,
              scale: targetScale,
              transformOrigin: '50% 50%',
              rotation: 0,
              duration: 0.25,
              overwrite: 'auto'
            });

            // Fade out background text, glow, and bottom elements completely
            if (heroWordmark) gsap.to(heroWordmark, { opacity: 0, duration: 0.15, overwrite: 'auto' });
            if (heroTagline) gsap.to(heroTagline, { opacity: 0, duration: 0.15, overwrite: 'auto' });
            if (heroBottomBar) gsap.to(heroBottomBar, { opacity: 0, duration: 0.15, overwrite: 'auto' });
            if (heroGlow) gsap.to(heroGlow, { opacity: 0, duration: 0.15, overwrite: 'auto' });

            // Ensure lower body & belly are 100% full opacity (no cutoffs)
            if (mochiLowerBody) {
              gsap.to(mochiLowerBody, {
                opacity: 1,
                y: 0,
                duration: 0.2,
                overwrite: 'auto'
              });
            }

            // Glasses slide down nose, alert anime eyes
            this.setExpression('alert');

            // Reveal flanking story text on sides of Mochi (desktop/tablet)
            if (mochiSideTextWrap) {
              gsap.to(mochiSideTextWrap, {
                opacity: 1,
                y: 0,
                duration: 0.3,
                overwrite: 'auto'
              });
            }
          }
        }
        // -------------------------------------------------------------
        // STEP 2: SCROLL 2 -> ARCHITECTURE SECTION (0.30 - 0.50, center at 0.40)
        // -------------------------------------------------------------
        else if (p < 0.50) {
          const subP = (p - 0.30) / (0.40 - 0.30);
          const clampedSubP = Math.min(1, Math.max(0, subP));

          const bodyGroup = document.getElementById('mochiBodyGroup');
          if (bodyGroup) bodyGroup.removeAttribute('clip-path');

          const leftAnchor = getAnchorPos('anchorSlide2') || {
            x: isMobile ? (window.innerWidth / 2) - currentDims.halfW : window.innerWidth * 0.18,
            y: isMobile ? window.innerHeight * 0.25 - currentDims.halfH : window.innerHeight * 0.5 - currentDims.halfH
          };

          const targetStep1Scale = window.innerWidth <= 480 ? 3.8 : (window.innerWidth <= 1024 ? 2.6 : 2.7);
          const targetScale = gsap.utils.interpolate(targetStep1Scale, 0.95, clampedSubP);
          const fullCenterX = (window.innerWidth / 2) - currentDims.halfW;
          const fullCenterY = 95 + (currentDims.h * (targetStep1Scale - 1) / 2);

          const targetX = gsap.utils.interpolate(fullCenterX, leftAnchor.x, clampedSubP);
          const targetY = gsap.utils.interpolate(fullCenterY, leftAnchor.y, clampedSubP);

          gsap.to(actor, {
            x: targetX,
            y: targetY,
            scale: targetScale,
            transformOrigin: '50% 50%',
            rotation: isMobile ? 0 : gsap.utils.interpolate(0, 5, clampedSubP),
            duration: 0.25,
            overwrite: 'auto'
          });

          if (mochiLowerBody) {
            gsap.to(mochiLowerBody, { opacity: 1, y: 0, duration: 0.2, overwrite: 'auto' });
          }

          if (heroWordmark) gsap.set(heroWordmark, { opacity: 0 });
          if (heroTagline) gsap.set(heroTagline, { opacity: 0 });
          if (heroBottomBar) gsap.set(heroBottomBar, { opacity: 0 });
          if (heroGlow) gsap.set(heroGlow, { opacity: 0 });
          if (mochiSideTextWrap) gsap.to(mochiSideTextWrap, { opacity: 0, y: -20, duration: 0.2, overwrite: 'auto' });

          this.setExpression('proud');
          this.setCoinState(false);
        }
        // -------------------------------------------------------------
        // STEP 3: SCROLL 3 -> NEXT SECTION: RING BUFFER (0.50 - 0.70, center at 0.60)
        // -------------------------------------------------------------
        else if (p < 0.70) {
          const subP = (p - 0.50) / (0.60 - 0.50);
          const clampedSubP = Math.min(1, Math.max(0, subP));

          const centerAnchor = getAnchorPos('anchorSlide3') || {
            x: (window.innerWidth / 2) - currentDims.halfW,
            y: (window.innerHeight / 2) - currentDims.halfH
          };
          const leftAnchor = getAnchorPos('anchorSlide2') || {
            x: isMobile ? (window.innerWidth / 2) - currentDims.halfW : window.innerWidth * 0.18,
            y: isMobile ? window.innerHeight * 0.25 - currentDims.halfH : window.innerHeight * 0.5 - currentDims.halfH
          };

          const targetX = gsap.utils.interpolate(leftAnchor.x, centerAnchor.x, clampedSubP);
          const targetY = gsap.utils.interpolate(leftAnchor.y, centerAnchor.y, clampedSubP);

          gsap.to(actor, {
            x: targetX,
            y: targetY,
            scale: 1.0,
            rotation: Math.sin(clampedSubP * Math.PI * 4) * 2,
            duration: 0.25,
            overwrite: 'auto'
          });

          if (mochiLowerBody) {
            gsap.to(mochiLowerBody, { opacity: 1, y: 0, duration: 0.2, overwrite: 'auto' });
          }

          this.setExpression('focused');
          this.setCoinState(false);
        }
        // -------------------------------------------------------------
        // STEP 4: SCROLL 4 -> PRIVACY SHADER MASKING (0.70 - 0.90, center at 0.80)
        // -------------------------------------------------------------
        else if (p < 0.90) {
          const subP = (p - 0.70) / (0.80 - 0.70);
          const clampedSubP = Math.min(1, Math.max(0, subP));

          const rightAnchor = getAnchorPos('anchorSlide4') || {
            x: isMobile ? (window.innerWidth / 2) - currentDims.halfW : window.innerWidth * 0.75,
            y: isMobile ? window.innerHeight * 0.65 - currentDims.halfH : window.innerHeight * 0.5 - currentDims.halfH
          };
          const centerAnchor = getAnchorPos('anchorSlide3') || {
            x: (window.innerWidth / 2) - currentDims.halfW,
            y: (window.innerHeight / 2) - currentDims.halfH
          };

          const targetX = gsap.utils.interpolate(centerAnchor.x, rightAnchor.x, clampedSubP);
          const targetY = gsap.utils.interpolate(centerAnchor.y, rightAnchor.y, clampedSubP);
          const targetRotation = isMobile ? 0 : gsap.utils.interpolate(0, -5, clampedSubP);

          gsap.to(actor, {
            x: targetX,
            y: targetY,
            scale: 0.95,
            rotation: targetRotation,
            duration: 0.25,
            overwrite: 'auto'
          });

          if (mochiLowerBody) {
            gsap.to(mochiLowerBody, { opacity: 1, y: 0, duration: 0.2, overwrite: 'auto' });
          }

          this.setExpression('happy');
          this.setCoinState(false);
          const travelerStage = document.getElementById('mochiTravelerStage');
          if (travelerStage) travelerStage.classList.remove('behind-footer-frame');
        }
        // -------------------------------------------------------------
        // STEP 5: SCROLL 5 -> FINAL SECTION: 'D' FRAME PEEKING SETTLE (0.90 - 1.00)
        // -------------------------------------------------------------
        else {
          const subP = (p - 0.90) / (1.00 - 0.90);
          const clampedSubP = Math.min(1, Math.max(0, subP));

          const dPortalEl = document.getElementById('anchorSlide5');
          const dPortalPos = dPortalEl ? dPortalEl.getBoundingClientRect() : null;
          // Target centered right inside the aperture of 'D'
          const coinAnchor = dPortalPos && dPortalPos.width > 0 ? {
            x: dPortalPos.left + (dPortalPos.width / 2) - currentDims.halfW,
            y: dPortalPos.top + (dPortalPos.height / 2) - currentDims.halfH
          } : {
            x: (window.innerWidth / 2) - currentDims.halfW,
            y: window.innerHeight * 0.35 - currentDims.halfH
          };

          const rightAnchor = getAnchorPos('anchorSlide4') || {
            x: isMobile ? (window.innerWidth / 2) - currentDims.halfW : window.innerWidth * 0.75,
            y: isMobile ? window.innerHeight * 0.65 - currentDims.halfH : window.innerHeight * 0.5 - currentDims.halfH
          };

          const targetX = gsap.utils.interpolate(rightAnchor.x, coinAnchor.x, clampedSubP);
          const targetY = gsap.utils.interpolate(rightAnchor.y, coinAnchor.y, clampedSubP);
          // Scale to fit snuggly inside the 'D' portal aperture: smaller, peeking out from the D frame
          const targetFinalScale = dPortalPos && dPortalPos.height > 0 ? (dPortalPos.height / currentDims.h) * 1.05 : (isMobile ? 0.28 : 0.42);
          const targetScale = gsap.utils.interpolate(0.95, targetFinalScale, clampedSubP);

          gsap.to(actor, {
            x: targetX,
            y: targetY,
            scale: targetScale,
            transformOrigin: '50% 50%',
            rotation: 0,
            duration: 0.25,
            overwrite: 'auto'
          });

          // Toggle behind-footer-frame class so Mochi sits behind the D letter body
          const travelerStage = document.getElementById('mochiTravelerStage');
          if (travelerStage) {
            travelerStage.classList.add('behind-footer-frame');
          }

          // Ensure Mochi character stays 100% visible
          const bodyGroup = document.getElementById('mochiBodyGroup');
          if (bodyGroup) {
            bodyGroup.style.opacity = '1';
            bodyGroup.removeAttribute('clip-path');
          }
          if (mochiLowerBody) {
            gsap.to(mochiLowerBody, { opacity: 1, y: 0, duration: 0.2, overwrite: 'auto' });
          }

          this.setExpression('wink');
          this.setCoinState(false);
        }
      }
    });
  }

  setCoinState(isCoin) {
    if (this.isCoin === isCoin) return;
    this.isCoin = isCoin;

    const coinPlate = document.getElementById('mochiCoinPlate');
    const actor = document.getElementById('mochiActor');

    if (isCoin) {
      if (actor) actor.classList.add('as-coin');
      if (coinPlate && typeof gsap !== 'undefined') {
        gsap.to(coinPlate, { opacity: 1, scale: 1, duration: 0.5, ease: 'back.out(1.8)' });
      }
    } else {
      if (actor) actor.classList.remove('as-coin');
      if (coinPlate && typeof gsap !== 'undefined') {
        gsap.to(coinPlate, { opacity: 0, scale: 0.8, duration: 0.3, ease: 'power2.in' });
      }
    }
  }
}

// Global initialization
document.addEventListener('DOMContentLoaded', () => {
  // 1. Theme Toggle
  const themeBtn = document.getElementById('themeToggleBtn');
  const storedTheme = localStorage.getItem('rewind_theme') || 'dark';

  function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('rewind_theme', theme);
    if (themeBtn) {
      themeBtn.innerHTML = theme === 'dark' 
        ? '<i class="ph ph-sun"></i>' 
        : '<i class="ph ph-moon"></i>';
    }
  }
  applyTheme(storedTheme);

  if (themeBtn) {
    themeBtn.addEventListener('click', () => {
      const active = document.documentElement.getAttribute('data-theme') || 'dark';
      applyTheme(active === 'dark' ? 'light' : 'dark');
    });
  }

  // 2. Discrete Wheel & Touch Intent Controller (1 Single Scroll = Exactly 1 Complete Step)
  // Step list mapping to exact scroll progress / targets:
  // Step 0: Hero Resting (0.00)
  // Step 1: Mochi Face Close-Up (0.20) -> Dedicated Close-up beat (slideCloseup)
  // Step 2: Architecture Section (0.40) -> Slide 2 (Mochi docks left)
  // Step 3: Ring Buffer Pipeline (0.60) -> Slide 3 (Mochi docks center)
  // Step 4: Privacy Shader Masking (0.80) -> Slide 4 (Mochi docks right)
  // Step 5: Verified Golden Coin Settle (1.00) -> Slide 5 (Mochi coin settle)
  const maxScroll = () => document.documentElement.scrollHeight - window.innerHeight;
  const stepFractions = [0.00, 0.20, 0.40, 0.60, 0.80, 1.00];

  let currentStepIndex = 0;
  let isTransitioning = false;

  function getCurrentStep() {
    const currentProg = window.scrollY / (maxScroll() || 1);
    let closestIndex = 0;
    let closestDist = 999;
    stepFractions.forEach((frac, idx) => {
      const dist = Math.abs(currentProg - frac);
      if (dist < closestDist) {
        closestDist = dist;
        closestIndex = idx;
      }
    });
    return closestIndex;
  }

  function goToStep(targetIndex) {
    if (targetIndex < 0) targetIndex = 0;
    if (targetIndex >= stepFractions.length) targetIndex = stepFractions.length - 1;

    currentStepIndex = targetIndex;
    isTransitioning = true;

    const targetY = stepFractions[targetIndex] * maxScroll();

    gsap.to(window, {
      scrollTo: { y: targetY, autoKill: false },
      duration: 0.75,
      ease: 'power2.out',
      overwrite: 'all',
      onComplete: () => {
        setTimeout(() => {
          isTransitioning = false;
        }, 80);
      }
    });
  }

  // Intercept wheel events: single impulse triggers exact next / previous slide
  window.addEventListener('wheel', (e) => {
    e.preventDefault();
    if (isTransitioning) return;

    if (Math.abs(e.deltaY) < 15) return; // ignore minor trackpad jitter

    currentStepIndex = getCurrentStep();
    if (e.deltaY > 0) {
      goToStep(currentStepIndex + 1);
    } else {
      goToStep(currentStepIndex - 1);
    }
  }, { passive: false });

  // Keyboard Arrow / Space / Page navigation
  window.addEventListener('keydown', (e) => {
    if (['ArrowDown', 'PageDown', ' '].includes(e.key)) {
      e.preventDefault();
      if (!isTransitioning) {
        currentStepIndex = getCurrentStep();
        goToStep(currentStepIndex + 1);
      }
    } else if (['ArrowUp', 'PageUp'].includes(e.key)) {
      e.preventDefault();
      if (!isTransitioning) {
        currentStepIndex = getCurrentStep();
        goToStep(currentStepIndex - 1);
      }
    }
  });

  // Touch Swipe on mobile
  let touchStartY = 0;
  window.addEventListener('touchstart', (e) => {
    if (e.touches && e.touches.length > 0) {
      touchStartY = e.touches[0].clientY;
    }
  }, { passive: true });

  window.addEventListener('touchend', (e) => {
    if (isTransitioning) return;
    if (e.changedTouches && e.changedTouches.length > 0) {
      const touchEndY = e.changedTouches[0].clientY;
      const deltaY = touchStartY - touchEndY;
      if (Math.abs(deltaY) > 40) {
        currentStepIndex = getCurrentStep();
        if (deltaY > 0) {
          goToStep(currentStepIndex + 1);
        } else {
          goToStep(currentStepIndex - 1);
        }
      }
    }
  }, { passive: true });

  // 3. Dot Navigation Clicks
  document.querySelectorAll('.story-slide-dot').forEach((dot) => {
    dot.addEventListener('click', () => {
      const stepIdx = parseInt(dot.getAttribute('data-step') || dot.getAttribute('data-slide'), 10);
      goToStep(stepIdx);
    });
  });

  // 4. Interactive Cursor-Following Ambient Glow in Hero & Footer (Slide 1 & Slide 5)
  const setupAmbientGlow = (glowId, layerId) => {
    const ambientGlow = document.getElementById(glowId);
    const layerEl = document.getElementById(layerId);

    if (ambientGlow && layerEl) {
      layerEl.addEventListener('mousemove', (e) => {
        const rect = layerEl.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        ambientGlow.style.left = `${x}px`;
        ambientGlow.style.top = `${y}px`;
        ambientGlow.style.opacity = '1';
      });

      layerEl.addEventListener('mouseleave', () => {
        ambientGlow.style.opacity = '0.75';
      });
    }
  };

  setupAmbientGlow('heroAmbientGlow', 'heroTextLayer');
  setupAmbientGlow('footerAmbientGlow', 'footerTextLayer');

  // 5. Instantiate Mochi Controller
  window.deckMochi = new DeckMochiNavigator();
});

