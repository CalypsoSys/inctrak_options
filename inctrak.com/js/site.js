(function (global) {
  function parseTimingValue(value, fallbackSeconds) {
    if (typeof value !== "string" || value.trim() === "") {
      return fallbackSeconds * 1000;
    }

    var normalized = value.trim();
    if (normalized.endsWith("ms")) {
      var milliseconds = Number.parseFloat(normalized.slice(0, -2));
      return Number.isFinite(milliseconds) ? milliseconds : fallbackSeconds * 1000;
    }

    if (normalized.endsWith("s")) {
      normalized = normalized.slice(0, -1);
    }

    var seconds = Number.parseFloat(normalized);
    return Number.isFinite(seconds) ? seconds * 1000 : fallbackSeconds * 1000;
  }

  function buildFeedbackPayload(fields) {
    return {
      Created: "0001-01-01T00:00:00",
      EmailAddress: fields.emailAddress || "",
      Name: fields.name || "",
      Subject: fields.subject || "",
      Message: fields.message || "",
      MessageTypeFk: "8",
      FeedbackPk: 0,
      ClientData: null,
      MessageTypeFkNavigation: null
    };
  }

  function shouldEnableSticky(scrollY, triggerTop, offset) {
    return scrollY >= Math.max(0, triggerTop - offset);
  }

  function shouldAnimateEntry(entry) {
    return Boolean(entry && entry.isIntersecting);
  }

  function applyRevealState(element) {
    if (!element) {
      return;
    }

    element.style.visibility = "visible";
    element.style.transitionDelay =
      parseTimingValue(element.dataset.revealDelay, 0) + "ms";
    element.style.transitionDuration =
      parseTimingValue(element.dataset.revealDuration, 0.75) + "ms";
    element.classList.add("is-visible");
  }

  function applyLoadRevealState(element) {
    if (!element) {
      return;
    }

    element.style.visibility = "visible";
    element.style.transitionDelay =
      parseTimingValue(element.dataset.loadDelay, 0) + "ms";
    element.style.transitionDuration =
      parseTimingValue(element.dataset.loadDuration, 0.75) + "ms";
    element.classList.add("is-visible");
  }

  function toggleMenu(button, target, expanded) {
    if (!button || !target) {
      return;
    }

    button.setAttribute("aria-expanded", expanded ? "true" : "false");
    target.classList.toggle("show", expanded);
  }

  function initNavbar() {
    var button = global.document.querySelector(".navbar-toggler");
    if (!button) {
      return;
    }

    var targetSelector = button.getAttribute("data-target");
    if (!targetSelector) {
      return;
    }

    var target = global.document.querySelector(targetSelector);
    if (!target) {
      return;
    }

    button.addEventListener("click", function () {
      var expanded = button.getAttribute("aria-expanded") === "true";
      toggleMenu(button, target, !expanded);
    });

    target.querySelectorAll("a").forEach(function (link) {
      link.addEventListener("click", function () {
        if (global.innerWidth < 992) {
          toggleMenu(button, target, false);
        }
      });
    });
  }

  function initStickyHeader() {
    var stickyHost = global.document.querySelector(".addsticky");
    var trigger = global.document.querySelector(".aboutUs");
    if (!stickyHost || !trigger) {
      return;
    }

    function refreshSticky() {
      var triggerTop = trigger.getBoundingClientRect().top + global.scrollY;
      stickyHost.classList.toggle(
        "sticky",
        shouldEnableSticky(global.scrollY, triggerTop, 300)
      );
    }

    refreshSticky();
    global.addEventListener("scroll", refreshSticky, { passive: true });
    global.addEventListener("resize", refreshSticky);
  }

  function initSmoothScroll() {
    global.document.querySelectorAll("a[href^=\"#\"]").forEach(function (link) {
      link.addEventListener("click", function (event) {
        var href = link.getAttribute("href");
        if (!href || href === "#") {
          return;
        }

        var target = global.document.querySelector(href);
        if (!target) {
          return;
        }

        event.preventDefault();
        var targetTop = target.getBoundingClientRect().top + global.scrollY - 85;
        global.scrollTo({
          top: Math.max(0, targetTop),
          behavior: "smooth"
        });
      });
    });
  }

  function initAnimations() {
    var loadAnimatedItems = global.document.querySelectorAll(".load-reveal");
    var animatedItems = global.document.querySelectorAll(".scroll-reveal");
    if (!loadAnimatedItems.length && !animatedItems.length) {
      return;
    }

    if (loadAnimatedItems.length) {
      var startLoadReveal = function () {
        loadAnimatedItems.forEach(function (item) {
          applyLoadRevealState(item);
        });
      };

      if (typeof global.requestAnimationFrame === "function") {
        global.requestAnimationFrame(startLoadReveal);
      } else {
        global.setTimeout(startLoadReveal, 0);
      }
    }

    if (!animatedItems.length) {
      return;
    }

    if (!global.IntersectionObserver) {
      animatedItems.forEach(function (item) {
        applyRevealState(item);
      });
      return;
    }

    var observer = new global.IntersectionObserver(
      function (entries, currentObserver) {
        entries.forEach(function (entry) {
          if (!shouldAnimateEntry(entry)) {
            return;
          }

          var element = entry.target;
          applyRevealState(element);
          currentObserver.unobserve(element);
        });
      },
      {
        rootMargin: "0px 0px -12% 0px",
        threshold: 0.2
      }
    );

    animatedItems.forEach(function (item) {
      item.style.visibility = "hidden";
      observer.observe(item);
    });
  }

  function initContactForm() {
    var form = global.document.getElementById("learn_more_id");
    if (!form) {
      return;
    }

    form.addEventListener("submit", function (event) {
      event.preventDefault();

      var payload = buildFeedbackPayload({
        emailAddress: global.document.getElementById("EMAIL_ADDRESS_ID").value,
        name: global.document.getElementById("NAME_ID").value,
        subject: global.document.getElementById("SUBJECT_ID").value,
        message: global.document.getElementById("MESSAGE_ID").value
      });

      global
        .fetch("https://shared.inctrak.com/api/feedback/save_message/", {
          method: "POST",
          headers: {
            "Content-Type": "application/json;charset=UTF-8"
          },
          body: JSON.stringify(payload)
        })
        .then(function (response) {
          return response
            .json()
            .catch(function () {
              return { message: response.ok ? "Your message has been sent." : "Unable to submit your message right now." };
            })
            .then(function (data) {
              if (!response.ok) {
                throw new Error(data.message || "Unable to submit your message right now.");
              }

              global.alert(data.message || "Your message has been sent.");
              form.reset();
            });
        })
        .catch(function (error) {
          global.alert(error.message || "Unable to submit your message right now.");
        });
    });
  }

  function init() {
    if (!global.document) {
      return;
    }

    initNavbar();
    initStickyHeader();
    initSmoothScroll();
    initAnimations();
    initContactForm();
  }

  if (typeof module !== "undefined" && module.exports) {
    module.exports = {
      buildFeedbackPayload: buildFeedbackPayload,
      parseTimingValue: parseTimingValue,
      applyLoadRevealState: applyLoadRevealState,
      applyRevealState: applyRevealState,
      shouldAnimateEntry: shouldAnimateEntry,
      shouldEnableSticky: shouldEnableSticky
    };
  }

  if (global.document) {
    global.document.addEventListener("DOMContentLoaded", init);
  }
})(typeof window !== "undefined" ? window : globalThis);
