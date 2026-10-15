/*
 * Sakura WebGL background adapted from Anand Davaasuren (at80).
 * https://codepen.io/at80/pen/kyOdeK — MIT; see assets/licenses/sakura-MIT.txt.
 * Adaptations: paper palette, viewport sizing, particle budget, pause and reduced motion.
 */
(() => {
'use strict';
if (!document.body.classList.contains('portfolio')) return;
const canvas = document.createElement('canvas');
canvas.className = 'sakura-canvas';
canvas.setAttribute('aria-hidden', 'true');
document.body.prepend(canvas);
const vertexSource = `
    uniform mat4 uProjection;
    uniform mat4 uModelview;
    uniform vec3 uResolution;
    uniform vec3 uOffset;
    uniform vec3 uDOF;  //x:focus distance, y:focus radius, z:max radius
    uniform vec3 uFade; //x:start distance, y:half distance, z:near fade start

    attribute vec3 aPosition;
    attribute vec3 aEuler;
    attribute vec2 aMisc; //x:size, y:fade

    varying vec3 pposition;
    varying float psize;
    varying float palpha;
    varying float pdist;

    //varying mat3 rotMat;
    varying vec3 normX;
    varying vec3 normY;
    varying vec3 normZ;
    varying vec3 normal;

    varying float diffuse;
    varying float specular;
    varying float rstop;
    varying float distancefade;

    void main(void) {
        // Projection is based on vertical angle
        vec4 pos = uModelview * vec4(aPosition + uOffset, 1.0);
        gl_Position = uProjection * pos;
        gl_PointSize = aMisc.x * uProjection[1][1] / -pos.z * uResolution.y * 0.5;

        pposition = pos.xyz;
        psize = aMisc.x;
        pdist = length(pos.xyz);
        palpha = smoothstep(0.0, 1.0, (pdist - 0.1) / uFade.z);

        vec3 elrsn = sin(aEuler);
        vec3 elrcs = cos(aEuler);
        mat3 rotx = mat3(
            1.0, 0.0, 0.0,
            0.0, elrcs.x, elrsn.x,
            0.0, -elrsn.x, elrcs.x
        );
        mat3 roty = mat3(
            elrcs.y, 0.0, -elrsn.y,
            0.0, 1.0, 0.0,
            elrsn.y, 0.0, elrcs.y
        );
        mat3 rotz = mat3(
            elrcs.z, elrsn.z, 0.0,
            -elrsn.z, elrcs.z, 0.0,
            0.0, 0.0, 1.0
        );
        mat3 rotmat = rotx * roty * rotz;
        normal = rotmat[2];

        mat3 trrotm = mat3(
            rotmat[0][0], rotmat[1][0], rotmat[2][0],
            rotmat[0][1], rotmat[1][1], rotmat[2][1],
            rotmat[0][2], rotmat[1][2], rotmat[2][2]
        );
        normX = trrotm[0];
        normY = trrotm[1];
        normZ = trrotm[2];

        const vec3 lit = vec3(0.6917144638660746, 0.6917144638660746, -0.20751433915982237);
        float tmpdfs = dot(lit, normal);
        if(tmpdfs < 0.0) {
            normal = -normal;
            tmpdfs = dot(lit, normal);
        }
        diffuse = 0.4 + tmpdfs;

        vec3 eyev = normalize(-pos.xyz);
        if(dot(eyev, normal) > 0.0) {
            vec3 hv = normalize(eyev + lit);
            specular = pow(max(dot(hv, normal), 0.0), 20.0);
        }
        else {
            specular = 0.0;
        }
        rstop = clamp((abs(pdist - uDOF.x) - uDOF.y) / uDOF.z, 0.0, 1.0);
        rstop = pow(rstop, 0.5);
        //-0.69315 = ln(0.5)
        distancefade = min(1.0, exp((uFade.x - pdist) * 0.69315 / uFade.y));
    }
    `;
const fragmentSource = `
    #ifdef GL_ES
    //precision mediump float;
    precision highp float;
    #endif

    uniform vec3 uDOF;  //x:focus distance, y:focus radius, z:max radius
    uniform vec3 uFade; //x:start distance, y:half distance, z:near fade start

    const vec3 fadeCol = vec3(0.96, 0.95, 0.94);

    varying vec3 pposition;
    varying float psize;
    varying float palpha;
    varying float pdist;

    //varying mat3 rotMat;
    varying vec3 normX;
    varying vec3 normY;
    varying vec3 normZ;
    varying vec3 normal;

    varying float diffuse;
    varying float specular;
    varying float rstop;
    varying float distancefade;
    float ellipse(vec2 p, vec2 o, vec2 r) {
        vec2 lp = (p - o) / r;
        return length(lp) - 1.0;
    }

    void main(void) {
        vec3 p = vec3(gl_PointCoord - vec2(0.5, 0.5), 0.0) * 2.0;
        vec3 d = vec3(0.0, 0.0, -1.0);
        float nd = normZ.z; //dot(-normZ, d);
        if(abs(nd) < 0.0001) discard;

        float np = dot(normZ, p);
        vec3 tp = p + d * np / nd;
        vec2 coord = vec2(dot(normX, tp), dot(normY, tp));

        //angle = 15 degree
        const float flwrsn = 0.258819045102521;
        const float flwrcs = 0.965925826289068;
        mat2 flwrm = mat2(flwrcs, -flwrsn, flwrsn, flwrcs);
        vec2 flwrp = vec2(abs(coord.x), coord.y) * flwrm;

        float r;
        if(flwrp.x < 0.0) {
            r = ellipse(flwrp, vec2(0.065, 0.024) * 0.5, vec2(0.36, 0.96) * 0.5);
        }
        else {
            r = ellipse(flwrp, vec2(0.065, 0.024) * 0.5, vec2(0.58, 0.96) * 0.5);
        }

        if(r > rstop) discard;

        vec3 col = mix(vec3(1.0, 0.8, 0.75), vec3(1.0, 0.9, 0.87), r);
        float grady = mix(0.0, 1.0, pow(coord.y * 0.5 + 0.5, 0.35));
        col *= vec3(1.0, grady, grady);
        col *= mix(0.8, 1.0, pow(abs(coord.x), 0.3));
        col = col * diffuse + specular;

        col = mix(fadeCol, col, distancefade);

        float alpha = (rstop > 0.001)? (0.5 - r / (rstop * 2.0)) : 1.0;
        alpha = smoothstep(0.0, 1.0, alpha) * palpha;
        gl_FragColor = vec4(col, alpha);
    }
    `;
                    // Utilities
    var Vector3 = {};
    var Matrix44 = {};
    Vector3.create = function(x, y, z) {
        return {'x':x, 'y':y, 'z':z};
    };
    Vector3.dot = function (v0, v1) {
        return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z;
    };
    Vector3.cross = function (v, v0, v1) {
        v.x = v0.y * v1.z - v0.z * v1.y;
        v.y = v0.z * v1.x - v0.x * v1.z;
        v.z = v0.x * v1.y - v0.y * v1.x;
    };
    Vector3.normalize = function (v) {
        var l = v.x * v.x + v.y * v.y + v.z * v.z;
        if(l > 0.00001) {
            l = 1.0 / Math.sqrt(l);
            v.x *= l;
            v.y *= l;
            v.z *= l;
        }
    };
    Vector3.arrayForm = function(v) {
        if(v.array) {
            v.array[0] = v.x;
            v.array[1] = v.y;
            v.array[2] = v.z;
        }
        else {
            v.array = new Float32Array([v.x, v.y, v.z]);
        }
        return v.array;
    };
    Matrix44.createIdentity = function () {
        return new Float32Array([1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0]);
    };
    Matrix44.loadProjection = function (m, aspect, vdeg, near, far) {
        var h = near * Math.tan(vdeg * Math.PI / 180.0 * 0.5) * 2.0;
        var w = h * aspect;

        m[0] = 2.0 * near / w;
        m[1] = 0.0;
        m[2] = 0.0;
        m[3] = 0.0;

        m[4] = 0.0;
        m[5] = 2.0 * near / h;
        m[6] = 0.0;
        m[7] = 0.0;

        m[8] = 0.0;
        m[9] = 0.0;
        m[10] = -(far + near) / (far - near);
        m[11] = -1.0;

        m[12] = 0.0;
        m[13] = 0.0;
        m[14] = -2.0 * far * near / (far - near);
        m[15] = 0.0;
    };
    Matrix44.loadLookAt = function (m, vpos, vlook, vup) {
        var frontv = Vector3.create(vpos.x - vlook.x, vpos.y - vlook.y, vpos.z - vlook.z);
        Vector3.normalize(frontv);
        var sidev = Vector3.create(1.0, 0.0, 0.0);
        Vector3.cross(sidev, vup, frontv);
        Vector3.normalize(sidev);
        var topv = Vector3.create(1.0, 0.0, 0.0);
        Vector3.cross(topv, frontv, sidev);
        Vector3.normalize(topv);

        m[0] = sidev.x;
        m[1] = topv.x;
        m[2] = frontv.x;
        m[3] = 0.0;

        m[4] = sidev.y;
        m[5] = topv.y;
        m[6] = frontv.y;
        m[7] = 0.0;

        m[8] = sidev.z;
        m[9] = topv.z;
        m[10] = frontv.z;
        m[11] = 0.0;

        m[12] = -(vpos.x * m[0] + vpos.y * m[4] + vpos.z * m[8]);
        m[13] = -(vpos.x * m[1] + vpos.y * m[5] + vpos.z * m[9]);
        m[14] = -(vpos.x * m[2] + vpos.y * m[6] + vpos.z * m[10]);
        m[15] = 1.0;
    };

    //
    var timeInfo = {
        'start':0, 'prev':0, // Date
        'delta':0, 'elapsed':0 // Number(sec)
    };

    //
    var gl;
    var renderSpec = {
        'width':0,
        'height':0,
        'aspect':1,
        'array':new Float32Array(3),
        'halfWidth':0,
        'halfHeight':0,
        'halfArray':new Float32Array(3)
        // and some render targets. see setViewport()
    };
    renderSpec.setSize = function(w, h) {
        renderSpec.width = w;
        renderSpec.height = h;
        renderSpec.aspect = renderSpec.width / renderSpec.height;
        renderSpec.array[0] = renderSpec.width;
        renderSpec.array[1] = renderSpec.height;
        renderSpec.array[2] = renderSpec.aspect;

        renderSpec.halfWidth = Math.floor(w / 2);
        renderSpec.halfHeight = Math.floor(h / 2);
        renderSpec.halfArray[0] = renderSpec.halfWidth;
        renderSpec.halfArray[1] = renderSpec.halfHeight;
        renderSpec.halfArray[2] = renderSpec.halfWidth / renderSpec.halfHeight;
    };
    function compileShader(shtype, shsrc) {
    	var retsh = gl.createShader(shtype);

    	gl.shaderSource(retsh, shsrc);
    	gl.compileShader(retsh);

    	if(!gl.getShaderParameter(retsh, gl.COMPILE_STATUS)) {
    		var errlog = gl.getShaderInfoLog(retsh);
    		gl.deleteShader(retsh);
    		console.error(errlog);
    		return null;
    	}
    	return retsh;
    }

    function createShader(vtxsrc, frgsrc, uniformlist, attrlist) {
        var vsh = compileShader(gl.VERTEX_SHADER, vtxsrc);
        var fsh = compileShader(gl.FRAGMENT_SHADER, frgsrc);

        if(vsh == null || fsh == null) {
            return null;
        }

        var prog = gl.createProgram();
        gl.attachShader(prog, vsh);
        gl.attachShader(prog, fsh);

        gl.deleteShader(vsh);
        gl.deleteShader(fsh);

        gl.linkProgram(prog);
        if (!gl.getProgramParameter(prog, gl.LINK_STATUS)) {
            var errlog = gl.getProgramInfoLog(prog);
            console.error(errlog);
            return null;
        }

        if(uniformlist) {
            prog.uniforms = {};
            for(var i = 0; i < uniformlist.length; i++) {
                prog.uniforms[uniformlist[i]] = gl.getUniformLocation(prog, uniformlist[i]);
            }
        }

        if(attrlist) {
            prog.attributes = {};
            for(var i = 0; i < attrlist.length; i++) {
                var attr = attrlist[i];
                prog.attributes[attr] = gl.getAttribLocation(prog, attr);
            }
        }

        return prog;
    }
    function useShader(prog) {
        gl.useProgram(prog);
        for(var attr in prog.attributes) {
            gl.enableVertexAttribArray(prog.attributes[attr]);;
        }
    }

    function unuseShader(prog) {
        for(var attr in prog.attributes) {
            gl.disableVertexAttribArray(prog.attributes[attr]);;
        }
        gl.useProgram(null);
    }

    /////
    var projection = {
        'angle':60,
        'nearfar':new Float32Array([0.1, 100.0]),
        'matrix':Matrix44.createIdentity()
    };
    var camera = {
        'position':Vector3.create(0, 0, 100),
        'lookat':Vector3.create(0, 0, 0),
        'up':Vector3.create(0, 1, 0),
        'dof':Vector3.create(10.0, 4.0, 8.0),
        'matrix':Matrix44.createIdentity()
    };

    var pointFlower = {};
    var meshFlower = {};
    var sceneStandBy = false;

    var BlossomParticle = function () {
        this.velocity = new Array(3);
        this.rotation = new Array(3);
        this.position = new Array(3);
        this.euler = new Array(3);
        this.size = 1.0;
        this.alpha = 1.0;
        this.zkey = 0.0;
    };

    BlossomParticle.prototype.setVelocity = function (vx, vy, vz) {
        this.velocity[0] = vx;
        this.velocity[1] = vy;
        this.velocity[2] = vz;
    };

    BlossomParticle.prototype.setRotation = function (rx, ry, rz) {
        this.rotation[0] = rx;
        this.rotation[1] = ry;
        this.rotation[2] = rz;
    };
    BlossomParticle.prototype.setPosition = function (nx, ny, nz) {
        this.position[0] = nx;
        this.position[1] = ny;
        this.position[2] = nz;
    };

    BlossomParticle.prototype.setEulerAngles = function (rx, ry, rz) {
        this.euler[0] = rx;
        this.euler[1] = ry;
        this.euler[2] = rz;
    };

    BlossomParticle.prototype.setSize = function (s) {
        this.size = s;
    };

    BlossomParticle.prototype.update = function (dt, et) {
        this.position[0] += this.velocity[0] * dt;
        this.position[1] += this.velocity[1] * dt;
        this.position[2] += this.velocity[2] * dt;

        this.euler[0] += this.rotation[0] * dt;
        this.euler[1] += this.rotation[1] * dt;
        this.euler[2] += this.rotation[2] * dt;
    };

    function createPointFlowers() {
        // get point sizes
        var prm = gl.getParameter(gl.ALIASED_POINT_SIZE_RANGE);
        renderSpec.pointSize = {'min':prm[0], 'max':prm[1]};

        var vtxsrc = vertexSource;
        var frgsrc = fragmentSource;

        pointFlower.program = createShader(
            vtxsrc, frgsrc,
            ['uProjection', 'uModelview', 'uResolution', 'uOffset', 'uDOF', 'uFade'],
            ['aPosition', 'aEuler', 'aMisc']
        );

        useShader(pointFlower.program);
        pointFlower.offset = new Float32Array([0.0, 0.0, 0.0]);
        pointFlower.fader = Vector3.create(0.0, 10.0, 0.0);

        // paramerters: velocity[3], rotate[3]
        pointFlower.numFlowers = window.innerWidth <= 760 ? 160 : 240;
        pointFlower.particles = new Array(pointFlower.numFlowers);
        // vertex attributes {position[3], euler_xyz[3], size[1]}
        pointFlower.dataArray = new Float32Array(pointFlower.numFlowers * (3 + 3 + 2));
        pointFlower.positionArrayOffset = 0;
        pointFlower.eulerArrayOffset = pointFlower.numFlowers * 3;
        pointFlower.miscArrayOffset = pointFlower.numFlowers * 6;

        pointFlower.buffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, pointFlower.buffer);
        gl.bufferData(gl.ARRAY_BUFFER, pointFlower.dataArray, gl.DYNAMIC_DRAW);
        gl.bindBuffer(gl.ARRAY_BUFFER, null);

        unuseShader(pointFlower.program);

        for(var i = 0; i < pointFlower.numFlowers; i++) {
            pointFlower.particles[i] = new BlossomParticle();
        }
    }
    function initPointFlowers() {
        //area
        pointFlower.area = Vector3.create(20.0, 20.0, 20.0);
        pointFlower.area.x = pointFlower.area.y * renderSpec.aspect;

        pointFlower.fader.x = 10.0; //env fade start
        pointFlower.fader.y = pointFlower.area.z; //env fade half
        pointFlower.fader.z = 0.1;  //near fade start

        //particles
        var PI2 = Math.PI * 2.0;
        var tmpv3 = Vector3.create(0, 0, 0);
        var tmpv = 0;
        var symmetryrand = function() {return (Math.random() * 2.0 - 1.0);};
        for(var i = 0; i < pointFlower.numFlowers; i++) {
            var tmpprtcl = pointFlower.particles[i];

            //velocity
            tmpv3.x = symmetryrand() * 0.3 + 0.8;
            tmpv3.y = symmetryrand() * 0.2 - 1.0;
            tmpv3.z = symmetryrand() * 0.3 + 0.5;
            Vector3.normalize(tmpv3);
            tmpv = 2.0 + Math.random() * 1.0;
            tmpprtcl.setVelocity(tmpv3.x * tmpv, tmpv3.y * tmpv, tmpv3.z * tmpv);

            //rotation
            tmpprtcl.setRotation(
                symmetryrand() * PI2 * 0.5,
                symmetryrand() * PI2 * 0.5,
                symmetryrand() * PI2 * 0.5
            );

            //position
            tmpprtcl.setPosition(
                symmetryrand() * pointFlower.area.x,
                symmetryrand() * pointFlower.area.y,
                symmetryrand() * pointFlower.area.z
            );

            //euler
            tmpprtcl.setEulerAngles(
                Math.random() * Math.PI * 2.0,
                Math.random() * Math.PI * 2.0,
                Math.random() * Math.PI * 2.0
            );

            //size
            tmpprtcl.setSize(0.5 + Math.random() * 0.1);
        }
    }

    function renderPointFlowers() {
        //update
        var PI2 = Math.PI * 2.0;
        var limit = [pointFlower.area.x, pointFlower.area.y, pointFlower.area.z];
        var repeatPos = function (prt, cmp, limit) {
            if(Math.abs(prt.position[cmp]) - prt.size * 0.5 > limit) {
                //out of area
                if(prt.position[cmp] > 0) {
                    prt.position[cmp] -= limit * 2.0;
                }
                else {
                    prt.position[cmp] += limit * 2.0;
                }
            }
        };
        var repeatEuler = function (prt, cmp) {
            prt.euler[cmp] = prt.euler[cmp] % PI2;
            if(prt.euler[cmp] < 0.0) {
                prt.euler[cmp] += PI2;
            }
        };

        for(var i = 0; i < pointFlower.numFlowers; i++) {
            var prtcl = pointFlower.particles[i];
            prtcl.update(timeInfo.delta, timeInfo.elapsed);
            repeatPos(prtcl, 0, pointFlower.area.x);
            repeatPos(prtcl, 1, pointFlower.area.y);
            repeatPos(prtcl, 2, pointFlower.area.z);
            repeatEuler(prtcl, 0);
            repeatEuler(prtcl, 1);
            repeatEuler(prtcl, 2);

            prtcl.alpha = 1.0;//(pointFlower.area.z - prtcl.position[2]) * 0.5;

            prtcl.zkey = (camera.matrix[2] * prtcl.position[0]
                        + camera.matrix[6] * prtcl.position[1]
                        + camera.matrix[10] * prtcl.position[2]
                        + camera.matrix[14]);
        }

        // sort
        pointFlower.particles.sort(function(p0, p1){return p0.zkey - p1.zkey;});

        // update data
        var ipos = pointFlower.positionArrayOffset;
        var ieuler = pointFlower.eulerArrayOffset;
        var imisc = pointFlower.miscArrayOffset;
        for(var i = 0; i < pointFlower.numFlowers; i++) {
            var prtcl = pointFlower.particles[i];
            pointFlower.dataArray[ipos] = prtcl.position[0];
            pointFlower.dataArray[ipos + 1] = prtcl.position[1];
            pointFlower.dataArray[ipos + 2] = prtcl.position[2];
            ipos += 3;
            pointFlower.dataArray[ieuler] = prtcl.euler[0];
            pointFlower.dataArray[ieuler + 1] = prtcl.euler[1];
            pointFlower.dataArray[ieuler + 2] = prtcl.euler[2];
            ieuler += 3;
            pointFlower.dataArray[imisc] = prtcl.size;
            pointFlower.dataArray[imisc + 1] = prtcl.alpha;
            imisc += 2;
        }

        //draw
        gl.enable(gl.BLEND);
        //gl.disable(gl.DEPTH_TEST);
        gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);

        var prog = pointFlower.program;
        useShader(prog);

        gl.uniformMatrix4fv(prog.uniforms.uProjection, false, projection.matrix);
        gl.uniformMatrix4fv(prog.uniforms.uModelview, false, camera.matrix);
        gl.uniform3fv(prog.uniforms.uResolution, renderSpec.array);
        gl.uniform3fv(prog.uniforms.uDOF, Vector3.arrayForm(camera.dof));
        gl.uniform3fv(prog.uniforms.uFade, Vector3.arrayForm(pointFlower.fader));

        gl.bindBuffer(gl.ARRAY_BUFFER, pointFlower.buffer);
        gl.bufferData(gl.ARRAY_BUFFER, pointFlower.dataArray, gl.DYNAMIC_DRAW);

        gl.vertexAttribPointer(prog.attributes.aPosition, 3, gl.FLOAT, false, 0, pointFlower.positionArrayOffset * Float32Array.BYTES_PER_ELEMENT);
        gl.vertexAttribPointer(prog.attributes.aEuler, 3, gl.FLOAT, false, 0, pointFlower.eulerArrayOffset * Float32Array.BYTES_PER_ELEMENT);
        gl.vertexAttribPointer(prog.attributes.aMisc, 2, gl.FLOAT, false, 0, pointFlower.miscArrayOffset * Float32Array.BYTES_PER_ELEMENT);

        // doubler
        for(var i = 1; i < 2; i++) {
            var zpos = i * -2.0;
            pointFlower.offset[0] = pointFlower.area.x * -1.0;
            pointFlower.offset[1] = pointFlower.area.y * -1.0;
            pointFlower.offset[2] = pointFlower.area.z * zpos;
            gl.uniform3fv(prog.uniforms.uOffset, pointFlower.offset);
            gl.drawArrays(gl.POINTS, 0, pointFlower.numFlowers);

            pointFlower.offset[0] = pointFlower.area.x * -1.0;
            pointFlower.offset[1] = pointFlower.area.y *  1.0;
            pointFlower.offset[2] = pointFlower.area.z * zpos;
            gl.uniform3fv(prog.uniforms.uOffset, pointFlower.offset);
            gl.drawArrays(gl.POINTS, 0, pointFlower.numFlowers);

            pointFlower.offset[0] = pointFlower.area.x *  1.0;
            pointFlower.offset[1] = pointFlower.area.y * -1.0;
            pointFlower.offset[2] = pointFlower.area.z * zpos;
            gl.uniform3fv(prog.uniforms.uOffset, pointFlower.offset);
            gl.drawArrays(gl.POINTS, 0, pointFlower.numFlowers);

            pointFlower.offset[0] = pointFlower.area.x *  1.0;
            pointFlower.offset[1] = pointFlower.area.y *  1.0;
            pointFlower.offset[2] = pointFlower.area.z * zpos;
            gl.uniform3fv(prog.uniforms.uOffset, pointFlower.offset);
            gl.drawArrays(gl.POINTS, 0, pointFlower.numFlowers);
        }

        //main
        pointFlower.offset[0] = 0.0;
        pointFlower.offset[1] = 0.0;
        pointFlower.offset[2] = 0.0;
        gl.uniform3fv(prog.uniforms.uOffset, pointFlower.offset);
        gl.drawArrays(gl.POINTS, 0, pointFlower.numFlowers);

        gl.bindBuffer(gl.ARRAY_BUFFER, null);
        unuseShader(prog);

        gl.enable(gl.DEPTH_TEST);
        gl.disable(gl.BLEND);
    }


    const motionPreference = matchMedia('(prefers-reduced-motion: reduce)');
    const control = document.createElement('button');
    control.type = 'button';
    control.className = 'background-control';
    const preferenceKey = 'portfolio:sakura-paused';
    let savedPause = false;
    try { savedPause = sessionStorage.getItem(preferenceKey) === 'true'; } catch (_) { /* Storage may be disabled. */ }
    let paused = motionPreference.matches || savedPause;
    let frame = 0;
    let previous = 0;
    let elapsed = 0;
    let ready = false;

    function updateControl() {
        const icon = paused
            ? '<path d="m9 5 10 7-10 7Z" />'
            : '<path d="M8 5v14M16 5v14" />';
        control.innerHTML = `<svg class="background-control-icon" aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">${icon}</svg><span class="background-control-action">${paused ? '背景を再生' : '背景を停止'}</span>`;
        control.setAttribute('aria-label', paused ? '桜の背景アニメーションを再生する' : '桜の背景アニメーションを一時停止する');
        control.title = paused ? '桜の背景：停止中' : '桜の背景：再生中';
        control.dataset.paused = String(paused);
    }

    function draw() {
        Matrix44.loadLookAt(camera.matrix, camera.position, camera.lookat, camera.up);
        gl.viewport(0, 0, canvas.width, canvas.height);
        gl.clearColor(245 / 255, 244 / 255, 239 / 255, 1);
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
        gl.enable(gl.DEPTH_TEST);
        renderPointFlowers();
    }

    function animate(now) {
        frame = 0;
        if (paused || document.hidden || !ready) return;
        timeInfo.delta = previous ? Math.min((now - previous) / 1000, 0.05) : 0;
        previous = now;
        elapsed += timeInfo.delta;
        timeInfo.elapsed = elapsed;
        draw();
        frame = requestAnimationFrame(animate);
    }

    function syncAnimation() {
        cancelAnimationFrame(frame);
        frame = 0;
        previous = 0;
        updateControl();
        if (!paused && !document.hidden && ready) frame = requestAnimationFrame(animate);
    }

    function resize() {
        // Limit drawing-buffer size while retaining the original 3D petal shapes.
        const scale = Math.min(devicePixelRatio || 1, 1.25, 1600 / innerWidth, 1200 / innerHeight);
        canvas.width = Math.max(1, Math.round(innerWidth * scale));
        canvas.height = Math.max(1, Math.round(innerHeight * scale));
        renderSpec.setSize(canvas.width, canvas.height);
        if (!ready) return;
        initPointFlowers();
        camera.position.z = pointFlower.area.z + projection.nearfar[0];
        projection.angle = Math.atan2(pointFlower.area.y, camera.position.z + pointFlower.area.z) * 180 / Math.PI * 2;
        Matrix44.loadProjection(projection.matrix, renderSpec.aspect, projection.angle, projection.nearfar[0], projection.nearfar[1]);
        timeInfo.delta = 0;
        draw();
    }

    try {
        gl = canvas.getContext('webgl', {alpha: false, antialias: false, powerPreference: 'low-power'});
        if (!gl) { canvas.remove(); return; }
        resize();
        createPointFlowers();
        if (!pointFlower.program) throw new Error('Sakura shader is unavailable');
        ready = true;
        resize();
        updateControl();
        const header = document.querySelector('.site-header');
        if (header) header.insertBefore(control, header.querySelector('.nav-toggle'));
        else document.querySelector('.page-footer-mini')?.append(control);
        syncAnimation();
    } catch (_) {
        canvas.remove();
        control.remove();
        return;
    }

    control.addEventListener('click', () => {
        paused = !paused;
        try { sessionStorage.setItem(preferenceKey, String(paused)); } catch (_) { /* The control works without storage. */ }
        syncAnimation();
    });
    motionPreference.addEventListener('change', event => { paused = event.matches; syncAnimation(); });
    document.addEventListener('visibilitychange', syncAnimation);
    window.addEventListener('resize', resize, {passive: true});
    canvas.addEventListener('webglcontextlost', event => {
        event.preventDefault();
        ready = false;
        syncAnimation();
        canvas.remove();
        control.remove();
    });
    window.addEventListener('pagehide', () => { cancelAnimationFrame(frame); frame = 0; previous = 0; });
    window.addEventListener('pageshow', syncAnimation);
})();
