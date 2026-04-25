function setActiveNavLink(activeId) {
  document.querySelectorAll('.navbar-nav .nav-item').forEach((item) => {
    const link = item.querySelector('.nav-link[href^="#"]');
    if (!link) {
      return;
    }

    const targetId = link.getAttribute('href')?.slice(1);
    item.classList.toggle('active', targetId === activeId);
  });
}

function initializeSmoothScroll() {
  const navLinks = document.querySelectorAll('.nav-link[href^="#"]');
  const toggler = document.querySelector('.navbar-toggler');
  const collapse = document.getElementById('navbarNav');

  navLinks.forEach((link) => {
    link.addEventListener('click', (event) => {
      const href = link.getAttribute('href');
      if (!href) {
        return;
      }

      const target = document.querySelector(href);
      if (!target) {
        return;
      }

      event.preventDefault();
      target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      setActiveNavLink(target.id);

      if (collapse && collapse.classList.contains('show')) {
        collapse.classList.remove('show');
        toggler?.setAttribute('aria-expanded', 'false');
      }
    });
  });
}

function initializeStickyHeader() {
  const stickyContainer = document.querySelector('.addsticky');
  const homeHeader = document.querySelector('header#home');

  if (!stickyContainer || !homeHeader) {
    return;
  }

  const observer = new IntersectionObserver(
    ([entry]) => {
      stickyContainer.classList.toggle('sticky', !entry.isIntersecting && entry.boundingClientRect.bottom <= 0);
    },
    {
      threshold: 0
    }
  );

  observer.observe(homeHeader);
}

function initializeRevealAnimations() {
  const animatedElements = document.querySelectorAll('.wow');
  if (!animatedElements.length) {
    return;
  }

  const revealElement = (element) => {
    if (element.classList.contains('animated')) {
      return;
    }

    const delay = element.getAttribute('data-wow-delay');
    const duration = element.getAttribute('data-wow-duration');

    if (delay) {
      element.style.animationDelay = delay;
    }

    if (duration) {
      element.style.animationDuration = duration;
    }

    element.classList.add('animated');
  };

  const shouldRevealElement = (element) => {
    const rect = element.getBoundingClientRect();
    const defaultTrigger = element.closest('header#home')
      ? window.innerHeight
      : window.innerHeight * 0.55;
    const offset = Number(element.getAttribute('data-wow-offset') || '0');
    const triggerLine = defaultTrigger - offset;
    return rect.top <= triggerLine && rect.bottom > 0;
  };

  const heroElements = Array.from(animatedElements).filter((element) => element.closest('header#home'));
  const scrollRevealElements = Array.from(animatedElements).filter((element) => !element.closest('header#home'));

  requestAnimationFrame(() => {
    heroElements.forEach((element) => revealElement(element));
  });

  const revealObserver = new IntersectionObserver(
    (entries, observer) => {
      entries.forEach((entry) => {
        if (!entry.isIntersecting && !shouldRevealElement(entry.target)) {
          return;
        }

        revealElement(entry.target);
        observer.unobserve(entry.target);
      });
    },
    {
      threshold: 0.2,
      rootMargin: '0px 0px -10% 0px'
    }
  );

  scrollRevealElements.forEach((element) => {
    if (shouldRevealElement(element)) {
      revealElement(element);
      return;
    }

    revealObserver.observe(element);
  });
}

function initializeMobileMenu() {
  const toggler = document.querySelector('.navbar-toggler');
  const collapse = document.getElementById('navbarNav');

  if (!toggler || !collapse) {
    return;
  }

  toggler.addEventListener('click', () => {
    const isExpanded = collapse.classList.toggle('show');
    toggler.setAttribute('aria-expanded', String(isExpanded));
  });
}

function initializeActiveSectionTracking() {
  const sections = Array.from(document.querySelectorAll('section[id], header[id]'));
  if (!sections.length) {
    return;
  }

  const observer = new IntersectionObserver(
    (entries) => {
      const visibleEntries = entries
        .filter((entry) => entry.isIntersecting)
        .sort((left, right) => right.intersectionRatio - left.intersectionRatio);

      if (visibleEntries.length) {
        setActiveNavLink(visibleEntries[0].target.id);
      }
    },
    {
      threshold: [0.25, 0.5, 0.75],
      rootMargin: '-120px 0px -35% 0px'
    }
  );

  sections.forEach((section) => observer.observe(section));
}

async function submitContactForm(event) {
  event.preventDefault();

  const emailAddress = document.getElementById('EMAIL_ADDRESS_ID');
  const name = document.getElementById('NAME_ID');
  const subject = document.getElementById('SUBJECT_ID');
  const message = document.getElementById('MESSAGE_ID');

  if (!(emailAddress instanceof HTMLInputElement) ||
      !(name instanceof HTMLInputElement) ||
      !(subject instanceof HTMLInputElement) ||
      !(message instanceof HTMLTextAreaElement)) {
    return;
  }

  const formData = {
    Created: '0001-01-01T00:00:00',
    EmailAddress: emailAddress.value,
    Name: name.value,
    Subject: subject.value,
    Message: message.value,
    MessageTypeFk: '8',
    FeedbackPk: 0,
    ClientData: null,
    MessageTypeFkNavigation: null
  };

  try {
    const response = await fetch('https://shared.inctrak.com/api/feedback/save_message/', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json;charset=UTF-8'
      },
      body: JSON.stringify(formData)
    });

    const data = await response.json().catch(() => ({}));

    if (!response.ok) {
      throw new Error(data.message || 'Unable to submit your message right now.');
    }

    alert(data.message || 'Your message has been sent.');
    emailAddress.value = '';
    name.value = '';
    subject.value = '';
    message.value = '';
  } catch (error) {
    alert(error instanceof Error ? error.message : 'Unable to submit your message right now.');
  }
}

function initializeContactForm() {
  const form = document.getElementById('learn_more_id');
  form?.addEventListener('submit', submitContactForm);
}

document.addEventListener('DOMContentLoaded', () => {
  initializeMobileMenu();
  initializeSmoothScroll();
  initializeStickyHeader();
  initializeRevealAnimations();
  initializeActiveSectionTracking();
  initializeContactForm();
  setActiveNavLink('home');
});
