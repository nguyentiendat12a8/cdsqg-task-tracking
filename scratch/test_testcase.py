class SourcePage:
    def __init__(self, number, text):
        self.Number = number
        self.Text = text

def test():
    pages = [
        SourcePage(66, "Viet Nam improved in digital government."),
        SourcePage(69, "Viet Nam has high OSI."),
        SourcePage(73, "Viet Nam has very high TII."),
        SourcePage(204, "2024 Annex EGDI\nRank LOSI\nRank Viet Nam V1 71 53 0.7709")
    ]
    
    entity_terms = ["viet nam", "vietnam", "việt nam"]
    terms = ["egdi", "e-government", "survey", "ranking", "rank", "annex"]
    
    scored = []
    for p in pages:
        t = p.Text.lower()
        has_ent = any(e in t for e in entity_terms)
        hits = sum(1 for term in terms if term in t)
        score = 0
        if has_ent and hits > 0:
            score = 100 + hits * 10
        elif has_ent:
            score = 50 + hits * 5
        elif hits > 0:
            score = hits * 5
        
        if score > 0:
            if "table" in t or "bảng" in t or "annex" in t: score += 15
            if "rank" in t or "ranking" in t: score += 10
            scored.append((p.Number, score, has_ent))
            
    scored.sort(key=lambda x: (x[2], x[1]), reverse=True)
    print("Scored pages:", scored)
    
    selected_focals = []
    for focal, score, has_ent in scored:
        # Check if already covered by another focal within +- 3 pages
        if any(abs(focal - f) <= 3 for f in selected_focals):
            continue
        selected_focals.append(focal)
        if len(selected_focals) >= 3:
            break
            
    print("Selected focals:", selected_focals)
    
    result = []
    for focal in selected_focals:
        # Find all pages in pages within +- 3 pages of focal
        cluster_pages = [p for p in pages if abs(p.Number - focal) <= 3]
        cluster_pages.sort(key=lambda p: p.Number)
        
        if len(cluster_pages) == 1:
            txt = cluster_pages[0].Text
        else:
            txt = "\n\n".join(f"--- [Trang PDF {p.Number}] ---\n{p.Text}" for p in cluster_pages)
            
        result.append(SourcePage(focal, txt))
        
    print("Result count:", len(result))
    print("selected[0].Number:", result[0].Number)
    print("selected[0].Text == pages[3].Text:", result[0].Text == pages[3].Text)

test()
