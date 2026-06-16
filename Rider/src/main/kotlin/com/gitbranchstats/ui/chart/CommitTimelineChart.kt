package com.gitbranchstats.ui.chart

import com.intellij.util.ui.UIUtil
import java.awt.*
import java.awt.geom.Path2D
import java.time.LocalDate
import java.time.format.DateTimeFormatter
import java.time.temporal.ChronoUnit
import javax.swing.JPanel

class CommitTimelineChart : JPanel() {

    private var data: Map<String, Map<LocalDate, Int>> = emptyMap()
    private var startDate: LocalDate = LocalDate.now().minusDays(30)
    private var endDate: LocalDate = LocalDate.now()

    // Author color palette — readable in both light and dark themes
    private val palette = listOf(
        Color(0x4E9CD0), Color(0xE06C75), Color(0x98C379),
        Color(0xE5C07B), Color(0xC678DD), Color(0x56B6C2),
        Color(0xD19A66), Color(0x61AFEF), Color(0xBE5046), Color(0xABB2BF)
    )

    private val marginLeft = 50
    private val marginRight = 140
    private val marginTop = 20
    private val marginBottom = 45

    init {
        preferredSize = Dimension(600, 280)
        isOpaque = true
    }

    fun setData(newData: Map<String, Map<LocalDate, Int>>, start: LocalDate, end: LocalDate) {
        data = newData
        startDate = start
        endDate = end
        repaint()
    }

    override fun paintComponent(g: Graphics) {
        super.paintComponent(g)
        val g2 = g as Graphics2D
        g2.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON)
        g2.setRenderingHint(RenderingHints.KEY_TEXT_ANTIALIASING, RenderingHints.VALUE_TEXT_ANTIALIAS_ON)

        g2.color = UIUtil.getPanelBackground()
        g2.fillRect(0, 0, width, height)

        if (data.isEmpty()) {
            drawEmptyState(g2)
            return
        }

        val cw = width - marginLeft - marginRight
        val ch = height - marginTop - marginBottom
        if (cw <= 0 || ch <= 0) return

        val maxCommits = data.values.flatMap { it.values }.maxOrNull() ?: 1

        drawGrid(g2, cw, ch, maxCommits)
        drawSeries(g2, cw, ch, maxCommits)
        drawAxes(g2, cw, ch, maxCommits)
        drawLegend(g2, ch)
    }

    private fun drawGrid(g2: Graphics2D, cw: Int, ch: Int, maxCommits: Int) {
        val isDark = !com.intellij.ui.JBColor.isBright()
        g2.color = if (isDark) Color(0x3C3F41) else Color(0xE0E0E0)
        g2.stroke = BasicStroke(0.8f, BasicStroke.CAP_BUTT, BasicStroke.JOIN_BEVEL, 0f, floatArrayOf(3f, 3f), 0f)

        val gridLines = 5
        for (i in 0..gridLines) {
            val y = marginTop + ch - ch * i / gridLines
            g2.drawLine(marginLeft, y, marginLeft + cw, y)
        }
        g2.stroke = BasicStroke(1f)
    }

    private fun drawSeries(g2: Graphics2D, cw: Int, ch: Int, maxCommits: Int) {
        val totalDays = ChronoUnit.DAYS.between(startDate, endDate).toInt().coerceAtLeast(1)
        val dayW = cw.toDouble() / totalDays
        val baseY = (marginTop + ch).toDouble()
        val authors = data.keys.toList()

        authors.forEachIndexed { idx, author ->
            val commits = data[author] ?: return@forEachIndexed
            val color = palette[idx % palette.size]

            // Build full point list for every day (0 for empty days)
            val pts = mutableListOf<DoubleArray>()
            var dayIdx = 0
            var date = startDate
            while (!date.isAfter(endDate)) {
                val count = commits[date] ?: 0
                val px = marginLeft + dayIdx * dayW + dayW / 2
                val py = marginTop + ch - ch * count.toDouble() / maxCommits
                pts.add(doubleArrayOf(px, py.coerceIn(marginTop.toDouble(), baseY)))
                date = date.plusDays(1)
                dayIdx++
            }
            if (pts.size < 2) return@forEachIndexed

            // Catmull-Rom spline → cubic bezier segments
            val path = Path2D.Double()
            path.moveTo(pts[0][0], pts[0][1])
            for (i in 0 until pts.size - 1) {
                val p0 = pts[(i - 1).coerceAtLeast(0)]
                val p1 = pts[i]
                val p2 = pts[i + 1]
                val p3 = pts[(i + 2).coerceAtMost(pts.size - 1)]
                val cp1x = p1[0] + (p2[0] - p0[0]) / 6.0
                val cp1y = (p1[1] + (p2[1] - p0[1]) / 6.0).coerceIn(marginTop.toDouble(), baseY)
                val cp2x = p2[0] - (p3[0] - p1[0]) / 6.0
                val cp2y = (p2[1] - (p3[1] - p1[1]) / 6.0).coerceIn(marginTop.toDouble(), baseY)
                path.curveTo(cp1x, cp1y, cp2x, cp2y, p2[0], p2[1])
            }

            g2.color = color
            g2.stroke = BasicStroke(2f, BasicStroke.CAP_ROUND, BasicStroke.JOIN_ROUND)
            g2.draw(path)

            // Dots only on days with actual commits
            date = startDate
            dayIdx = 0
            while (!date.isAfter(endDate)) {
                val count = commits[date] ?: 0
                if (count > 0) {
                    val px = (marginLeft + dayIdx * dayW + dayW / 2).toInt()
                    val py = (marginTop + ch - ch * count.toDouble() / maxCommits).toInt()
                    g2.fillOval(px - 4, py - 4, 8, 8)
                }
                date = date.plusDays(1)
                dayIdx++
            }
            g2.stroke = BasicStroke(1f)
        }
    }

    private fun drawAxes(g2: Graphics2D, cw: Int, ch: Int, maxCommits: Int) {
        val fg = UIUtil.getLabelForeground()
        g2.color = fg
        g2.stroke = BasicStroke(1.5f)
        g2.drawLine(marginLeft, marginTop, marginLeft, marginTop + ch)
        g2.drawLine(marginLeft, marginTop + ch, marginLeft + cw, marginTop + ch)
        g2.stroke = BasicStroke(1f)

        val smallFont = g2.font.deriveFont(10f)
        g2.font = smallFont
        val fm = g2.getFontMetrics(smallFont)

        // Y-axis labels
        val gridLines = 5
        for (i in 0..gridLines) {
            val value = maxCommits * i / gridLines
            val y = marginTop + ch - ch * i / gridLines
            val label = value.toString()
            g2.drawString(label, marginLeft - fm.stringWidth(label) - 6, y + fm.ascent / 2)
        }

        // X-axis date labels
        val totalDays = ChronoUnit.DAYS.between(startDate, endDate).toInt().coerceAtLeast(1)
        val dayW = cw.toDouble() / totalDays
        val labelEvery = when {
            totalDays <= 31 -> 5
            totalDays <= 93 -> 14
            totalDays <= 366 -> 30
            else -> 60
        }
        val fmt = DateTimeFormatter.ofPattern("MM/dd")
        var dayIdx = 0
        var date = startDate
        while (!date.isAfter(endDate)) {
            if (dayIdx % labelEvery == 0) {
                val px = (marginLeft + dayIdx * dayW + dayW / 2).toInt()
                val label = date.format(fmt)
                val lw = fm.stringWidth(label)
                g2.drawString(label, px - lw / 2, marginTop + ch + fm.ascent + 6)
            }
            date = date.plusDays(1)
            dayIdx++
        }

        // Y-axis title
        val titleFont = g2.font.deriveFont(10f)
        g2.font = titleFont
        val title = "commits/day"
        val titleFm = g2.getFontMetrics(titleFont)
        val tx = Graphics2D::class.java  // just for reference
        val savedTransform = g2.transform
        g2.rotate(-Math.PI / 2, (marginLeft - 30).toDouble(), (marginTop + ch / 2).toDouble())
        g2.drawString(title, marginLeft - 30 - titleFm.stringWidth(title) / 2, marginTop + ch / 2 + titleFm.ascent / 2)
        g2.transform = savedTransform
    }

    private fun drawLegend(g2: Graphics2D, ch: Int) {
        val authors = data.keys.toList()
        if (authors.isEmpty()) return

        val x = width - marginRight + 12
        var y = marginTop + 8
        val labelFont = g2.font.deriveFont(11f)
        g2.font = labelFont
        val fm = g2.getFontMetrics(labelFont)
        val lineH = fm.height + 6

        g2.color = UIUtil.getLabelForeground()
        g2.drawString("Authors", x, y + fm.ascent)
        y += lineH + 4

        authors.forEachIndexed { idx, author ->
            val color = palette[idx % palette.size]
            g2.color = color
            g2.fillRoundRect(x, y + 1, 12, 12, 3, 3)
            g2.color = UIUtil.getLabelForeground()
            val display = if (author.length > 17) author.take(15) + "…" else author
            g2.drawString(display, x + 18, y + fm.ascent)
            y += lineH
            if (y > marginTop + ch) return
        }
    }

    private fun drawEmptyState(g2: Graphics2D) {
        val msg = "No commit data — open a Git repository and refresh"
        g2.font = g2.font.deriveFont(12f)
        val fm = g2.getFontMetrics()
        g2.color = UIUtil.getInactiveTextColor()
        g2.drawString(msg, (width - fm.stringWidth(msg)) / 2, height / 2)
    }
}
